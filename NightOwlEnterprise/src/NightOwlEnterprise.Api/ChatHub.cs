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
    private readonly static Dictionary<string, (string name, List<string> connectionIds)> userIdentifierToConnectionIds =
        new Dictionary<string, (string name, List<string> connectionIds)>();
    
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
        
        var isReceiverIdExist = userIdentifierToConnectionIds.ContainsKey(receiverId);

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
        var name = this.Context.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name).Value;

        if (userIdentifier is null) return base.OnConnectedAsync();
        
        var isUserIdentifierExist = userIdentifierToConnectionIds.ContainsKey(userIdentifier);
        if (isUserIdentifierExist)
        {
            userIdentifierToConnectionIds[userIdentifier].connectionIds.Add(connectionId);
        }
        else
        {
            userIdentifierToConnectionIds.Add(userIdentifier, (name, new List<string>() { connectionId }));
        }
        
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }
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
}