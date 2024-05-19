using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Controllers;

[Route("[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class ChatController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IMongoCollection<Conversation> _conversationCollection;
    private readonly IMongoCollection<Message> _messagesCollection;
    
    public ChatController(UserManager<ApplicationUser> userManager,
        IMongoDatabase database)
    {
        this.userManager = userManager;
        _messagesCollection = database.GetCollection<Message>("messages");
        _conversationCollection = database.GetCollection<Conversation>("conversations");
    }
    
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet("users")]
    [Authorize]
    public IActionResult Users()
    {
        var strId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var id = Guid.Parse(strId);
        var users = userManager.Users.Where(x => x.Id != id).Select(x => new User(x.Id, x.Name)).ToList();
        return Json(users);
    }
    
    [HttpGet("meInfo")]
    [Authorize]
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
}

public record User(Guid id, string name);