using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = false)]
public class ChatController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IMongoCollection<Conversation> _conversationCollection;
    private readonly IMongoCollection<Message> _messagesCollection;

    public ChatController(UserManager<ApplicationUser> userManager, IMongoDatabase database)
    {
        this.userManager = userManager;
        _messagesCollection = database.GetCollection<Message>("messages");
        _conversationCollection = database.GetCollection<Conversation>("conversations");
    }
    
    // GET
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet("users")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Users()
    {
        var strId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var id = Guid.Parse(strId);
        var users = userManager.Users.Where(x => x.Id != id).Select(x => new User(x.Id, x.Name)).ToList();
        return Json(users);
    }
    
    [HttpGet("meInfo")]
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult MeInfo()
    {
        var strId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var id = Guid.Parse(strId);
        return Json(new
        {
            id = id
        });
    }
    
    [HttpGet("getMessages")]
    [Authorize]
    public async Task<IEnumerable<Message>> GetMessages(string receiverId, DateTime? lastMessageDate, int limit = 10)
    {
        var senderId = HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

        var conversation = await _conversationCollection
            .Find(c => c.Participants.Contains(senderId) && c.Participants.Contains(receiverId)).FirstOrDefaultAsync();

        if (conversation == null)
        {
            return new List<Message>();
        }
        
        FilterDefinition<Message> filter = null;
        
        if (lastMessageDate.HasValue)
        {
            filter = Builders<Message>.Filter.Where(message =>
                message.ConversationId == conversation.Id && message.Timestamp < lastMessageDate);
        }
        else
        {
            filter = Builders<Message>.Filter.Where(message => message.ConversationId == conversation.Id);
        }
        
        var sort = Builders<Message>.Sort.Descending(doc => doc.Timestamp); // Azalan sıralama
        
        // Sorguyu oluşturun ve uygulayın
        var messages = _messagesCollection.Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToList();
        
        return messages;
    }
    
    [HttpGet("getChats")]
    [Authorize]
    public async Task<PagedResponse<ChatInfo>> GetChats([FromQuery] int? page,[FromQuery] int? pageSize)
    {
        var paginationFilter = new PaginationFilter(page, pageSize);
        
        var senderId = HttpContext.User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

        var sort = Builders<Conversation>.Sort.Descending(c => c.LastMessageTimestamp);
        
        var conversations = await _conversationCollection
            .Find(c => c.Participants.Contains(senderId))
            .Sort(sort)
            .Skip((paginationFilter.PageNumber - 1) * paginationFilter.PageSize)
            .Limit(paginationFilter.PageSize)
            .ToListAsync();

        var totalCount = await _conversationCollection.CountDocumentsAsync(c => c.Participants.Contains(senderId));

        var studentIds = new List<Guid>();
        var chatInfoDict = new Dictionary<Guid, ChatInfo>();
        
        foreach (var conversation in conversations)
        {
            var lastMessageObj = conversation.LastMessage;
            
            if (lastMessageObj is null)
            {
                continue;
            }

            var strStudentId = lastMessageObj.SenderId == senderId
                ? lastMessageObj.ReceiverId
                : lastMessageObj.SenderId;

            if (!Guid.TryParse(strStudentId, out var studentId)) continue;
            
            studentIds.Add(studentId);
            
            chatInfoDict.Add(studentId, new ChatInfo()
            {
                StudentId = studentId,
                LastMessageDate = lastMessageObj.Timestamp,
                SystemMessage = lastMessageObj.SystemMessage,
                LastMessage = lastMessageObj.Content,
                LastMessageOwner = lastMessageObj.SenderId == senderId ? "Coach" : "Student"
            });
        }
        
        var applicationDbContext = HttpContext.RequestServices.GetService<ApplicationDbContext>();
        var paginationUriBuilder = HttpContext.RequestServices.GetService<PaginationUriBuilder>();
        
        var studentsSummarizedInfo = await applicationDbContext.StudentDetail.Where(x => studentIds.Contains(x.StudentId)).Select(x => new
        {
            Id = x.StudentId,
            Name = x.Name,
            Surname = x.Surname
        }).ToDictionaryAsync(x => x.Id, arg => arg);

        foreach (var chatInfoKeyValue in chatInfoDict)
        {
            var studentInfo = studentsSummarizedInfo[chatInfoKeyValue.Key];
            chatInfoKeyValue.Value.StudentFullName = $"{studentInfo.Name} {studentInfo.Surname}";
            chatInfoKeyValue.Value.ProfilePhoto = paginationUriBuilder.GetStudentProfilePhotoUri(chatInfoKeyValue.Key);
        }

        var chatInfoList = chatInfoDict.Values.ToList();

        var pagedResponse =
            PagedResponse<ChatInfo>.CreatePagedResponse(chatInfoList, (int)totalCount, paginationFilter,
                paginationUriBuilder, HttpContext.Request.Path.Value ?? string.Empty);

        return pagedResponse;
    }
    
    // GET
    [HttpGet("connectioninfo")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult ConnectionInfo()
    {
        return View();
    }
    
    [HttpGet("getConnections")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public ActionResult<IEnumerable<UserConnectionDto>> GetConnections(string userIdentifier = null, string email = null, string role = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var searchedUserIdentifier = new List<string>();
            
            if (!string.IsNullOrEmpty(email))
            {
                searchedUserIdentifier.AddRange(ChatHub.userIdentifierToUserInfo
                    .Where(x => x.Value.Email.Contains(email)).Select(x => x.Key));
            }
            
            if (!string.IsNullOrEmpty(role))
            {
                searchedUserIdentifier.AddRange(ChatHub.userIdentifierToUserInfo.Where(x => x.Value.Role.Contains(role))
                    .Select(x => x.Key));
            }
            
            // Tüm bağlantı bilgilerini al
            var pagedConnections = ChatHub.userIdentifierToConnections
                .Where(kv => userIdentifier == null || kv.Key == userIdentifier || searchedUserIdentifier.Contains(kv.Key))
                .Skip((page - 1) * pageSize).Take(pageSize)
                .SelectMany(kv => kv.Value.Select(ci => new UserConnectionDto
                {
                    UserIdentifier = kv.Key,
                    ConnectionId = ci.Key,
                    DisconnectedTime = ci.Value.DisconnectedTime,
                    CreatedAt = ci.Value.CreatedAt,
                    UpdatedAt = ci.Value.UpdatedAt,
                    CurrentState = ci.Value.CurrentState,
                    Email = ChatHub.userIdentifierToUserInfo[kv.Key].Email,
                    Name = ChatHub.userIdentifierToUserInfo[kv.Key].Name,
                    Role = ChatHub.userIdentifierToUserInfo[kv.Key].Role,
                })).ToList();

            // Sayfalama işlemi
            //var pagedConnections = allConnections.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(pagedConnections);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex}");
        }
    }
}

public class UserConnectionDto
{
    public string UserIdentifier { get; set; }
    public string ConnectionId { get; set; }
    
    public DateTime DisconnectedTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CurrentState { get; set; }
    
    public string Role { get; set; }
    
    public string Name { get; set; }
    
    public string Email { get; set; }
}

public record User(Guid id, string name);

public class ChatInfo
{
    public Guid StudentId { get; set; }
    
    public string ProfilePhoto { get; set; }
    
    public string StudentFullName { get; set; }
    
    public string LastMessage { get; set; }
    
    public string LastMessageOwner { get; set; }
    
    public DateTime LastMessageDate { get; set; }
    
    public SystemMessage SystemMessage { get; set; }
}
