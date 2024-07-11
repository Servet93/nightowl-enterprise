using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace NightOwlEnterprise.Api;

[Authorize]
public class ChatHub : Hub
{
    //user identifier -> connections [ connectionId, connectionInfo ],
    public readonly static Dictionary<string, Dictionary<string, ConnectionInfo>> userIdentifierToConnections = new();
    
    //user identifier -> user info
    public readonly static Dictionary<string, UserInfo> userIdentifierToUserInfo = new();

    private readonly IMongoCollection<Conversation> _conversationCollection;
    private readonly IMongoCollection<Message> _messageCollection;

    public ChatHub(IMongoDatabase database)
    {
        _conversationCollection = database.GetCollection<Conversation>("conversations");
        _messageCollection = database.GetCollection<Message>("messages");
    }
    
    // public async Task SendMessage(string user, string message)
    // {
    //     var connectionId = this.Context.ConnectionId;
    //     var userIdentifier = this.Context.UserIdentifier;
    //     var claims = this.Context.User.Claims.Select(x => $"{x.Type} -> {x.Value}").ToList();
    //     var additionalInfo = string.Join("\r\n", claims);
    //     
    //     await Clients.All.SendAsync("ReceiveMessage", user, message, connectionId, userIdentifier, additionalInfo);
    // }
    
    // public async Task SendMessage(string senderId, string receiverId, string message)
    public async Task SendMessage(string receiverId, string message)
    {
        var senderId = this.Context.UserIdentifier;
        var senderName = this.Context.User.Identity.Name;
        
        // Mesajı MongoDB'ye kaydet
        var conversation = await _conversationCollection.Find(c => c.Participants.Contains(senderId) && c.Participants.Contains(receiverId)).FirstOrDefaultAsync();
        
        if (conversation == null)
        {
            conversation = new Conversation { Participants = new List<string> { senderId, receiverId } };
            await _conversationCollection.InsertOneAsync(conversation);
        }

        var timeStamp  = DateTime.UtcNow;
        
        var messageObj = new Message
        {
            ConversationId = conversation.Id, SenderId = senderId, ReceiverId = receiverId, Content = message,
            Timestamp = timeStamp
        };
        
        await _messageCollection.InsertOneAsync(messageObj);
        
        //var isReceiverIdExist = userIdentifierToConnectionIds.ContainsKey(receiverId);

        var senderIds = new List<string>() { senderId, receiverId };
        
        // if (isReceiverIdExist)
        // {
        //     var receiverName = userIdentifierToConnectionIds[receiverId].name;
        //     senderIds.Add(receiverId);
        // }
        
        // Gönderen ve alıcıya mesajı gönder
        await Clients.Users(senderIds)
            .SendAsync("ReceiveMessage", senderId, receiverId, message, timeStamp);
    }
    
    public async Task SendMessageFromSystem(string senderId, string receiverId, string message)
    {
        // Mesajı MongoDB'ye kaydet
        var conversation = await _conversationCollection.Find(c => c.Participants.Contains(senderId) && c.Participants.Contains(receiverId)).FirstOrDefaultAsync();
        
        if (conversation == null)
        {
            conversation = new Conversation { Participants = new List<string> { senderId, receiverId } };
            await _conversationCollection.InsertOneAsync(conversation);
        }

        var timeStamp  = DateTime.UtcNow;
        var sentFromSystem = true;
        
        var messageObj = new Message
        {
            ConversationId = conversation.Id, SenderId = senderId, ReceiverId = receiverId, Content = message,
            Timestamp = timeStamp,
            SentFromSystem = sentFromSystem
        };
        
        await _messageCollection.InsertOneAsync(messageObj);
        
        //var isReceiverIdExist = userIdentifierToConnectionIds.ContainsKey(receiverId);

        var senderIds = new List<string>() { senderId, receiverId };
        
        // if (isReceiverIdExist)
        // {
        //     var receiverName = userIdentifierToConnectionIds[receiverId].name;
        //     senderIds.Add(receiverId);
        // }
        
        // Gönderen ve alıcıya mesajı gönder
        await Clients.Users(senderIds)
            .SendAsync("ReceiveMessage", senderId, receiverId, message, timeStamp, sentFromSystem);
    }

    // public async Task<IEnumerable<Message>> GetChatHistory(string userId, string partnerId, int pageNumber, int pageSize)
    // {
    //     var conversation = await _conversationCollection.Find(c => c.Participants.Contains(userId) && c.Participants.Contains(partnerId)).FirstOrDefaultAsync();
    //     if (conversation != null)
    //     {
    //         var startIndex = Math.Max(0, conversation.Messages.Count - ((pageNumber + 1) * pageSize));
    //         var endIndex = Math.Max(0, startIndex + pageSize);
    //         return conversation.Messages.GetRange(startIndex, endIndex - startIndex);
    //     }
    //     return null;
    // }

    public override Task OnConnectedAsync()
    {
        var connectionId = this.Context.ConnectionId;
        var userIdentifier = this.Context.UserIdentifier;
        
        if (userIdentifier is null) return base.OnConnectedAsync();
        
        var isUserIdentifierExist = userIdentifierToConnections.ContainsKey(userIdentifier);
        if (isUserIdentifierExist)
        {
            var isConnectionExistForUserIdentifier =
                userIdentifierToConnections[userIdentifier].ContainsKey(connectionId);

            if (isConnectionExistForUserIdentifier)
            {
                var connectionInfo = userIdentifierToConnections[userIdentifier][connectionId];
                connectionInfo.CurrentState = "Connected";
                connectionInfo.UpdatedAt = DateTime.UtcNow.ConvertUtcToTimeZone();
            }
            else
            {
                userIdentifierToConnections[userIdentifier].Add(connectionId, new ConnectionInfo()
                {
                    ConnectionId = connectionId,
                    CurrentState = "Connected",
                    CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone(),
                    UpdatedAt = DateTime.UtcNow.ConvertUtcToTimeZone(),
                });
            }
        }
        else
        {
            userIdentifierToConnections.Add(userIdentifier, new Dictionary<string, ConnectionInfo>()
            {
                {
                    connectionId, new ConnectionInfo()
                    {
                        ConnectionId = connectionId,
                        CurrentState = "Connected",
                        CreatedAt = DateTime.UtcNow.ConvertUtcToTimeZone(),
                        UpdatedAt = DateTime.UtcNow.ConvertUtcToTimeZone(),
                    }
                }
            });
        }
        
        if (!userIdentifierToUserInfo.ContainsKey(userIdentifier))
        {
            var name = this.Context.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value;
            var role = this.Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;
            var email = this.Context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
            
            userIdentifierToUserInfo.Add(userIdentifier, new UserInfo()
            {
                Role = role,
                Email = email,
                Name = name
            });
        }
        
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = this.Context.ConnectionId;
        var userIdentifier = this.Context.UserIdentifier;
        var name = this.Context.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value;

        if (userIdentifierToConnections.ContainsKey(userIdentifier))
        {
            if (userIdentifierToConnections[userIdentifier].ContainsKey(connectionId))
            {
                var connectionInfo = userIdentifierToConnections[userIdentifier][connectionId];
                connectionInfo.CurrentState = "Disconnected";
                connectionInfo.DisconnectedTime = DateTime.UtcNow.ConvertUtcToTimeZone();
                connectionInfo.UpdatedAt = DateTime.UtcNow.ConvertUtcToTimeZone();
            }
        }

        
        
        return base.OnDisconnectedAsync(exception);
    }
}

public class ConnectionInfo
{
    public string ConnectionId { get; set; }
    
    public DateTime DisconnectedTime { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    public string CurrentState { get; set; }
}

public class UserInfo
{
    public string Name { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }
}

// MongoDB Model
public class Conversation
{
    public ObjectId Id { get; set; }
    public List<string> Participants { get; set; }
}

public class Message
{
    public ObjectId Id { get; set; }
    public string SenderId { get; set; }
    public ObjectId ConversationId { get; set; }
    public string ReceiverId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    
    public bool SentFromSystem { get; set; }
}