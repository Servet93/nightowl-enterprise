using System.ComponentModel;
using System.Security.Claims;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Controllers;

[Route("[controller]")]
public class NotificationController : Controller
{
    private readonly ApplicationDbContext _applicationDbContext;

    public NotificationController(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }
    
    // GET
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
    {
        var message = new FirebaseAdmin.Messaging.Message()
        {
            Token = request.Token,
            Notification = new Notification
            {
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.ImageUrl
            }
        };

        var messaging = FirebaseMessaging.DefaultInstance;
        var result = await messaging.SendAsync(message);
        return Ok(new { MessageId = result });
    }
    
    [HttpPost("send-to-all-users")]
    public async Task<IActionResult> SendToAllUsersNotification([FromBody] NotificationAllUserRequest request)
    {
        var list = _applicationDbContext.UserDevices.ToList();

        foreach (var listItem in list)
        {
            var message = new FirebaseAdmin.Messaging.Message()
            {
                Token = listItem.DeviceToken,
                Notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl
                }
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            var result = await messaging.SendAsync(message);    
        }
        
        return Ok();
    }
    
    [HttpPost("update-device-token")]
    [Authorize]
    public async Task<IActionResult> UpdateDeviceToken([FromBody] UpdateDeviceTokenRequest request)
    {
        // Diğer talep bilgilerine erişmek için
        var userClaims = ((ClaimsIdentity)User.Identity).Claims;
        
        // Yetkili kullanıcının id'sini almak için
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Guid.TryParse(userId, out var gUserId);

        var user = _applicationDbContext.UserDevices.FirstOrDefault(x => x.UserId == gUserId);

        if (user is null)
        {
            _applicationDbContext.UserDevices.Add(new UserDevice()
            {
                UserId = gUserId,
                DeviceToken = request.DeviceToken,
                UpdatedAt = DateTime.UtcNow
            });    
        }
        else
        {
            user.DeviceToken = request.DeviceToken;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _applicationDbContext.SaveChangesAsync();
        
        // var message = new FirebaseAdmin.Messaging.Message()
        // {
        //     Token = request.Token,
        //     Notification = new Notification
        //     {
        //         Title = request.Title,
        //         Body = request.Body,
        //         ImageUrl = request.ImageUrl
        //     }
        // };
        //
        // var messaging = FirebaseMessaging.DefaultInstance;
        // var result = await messaging.SendAsync(message);
        return Ok();
    }
    
    public class NotificationRequest
    {
        public string Token { get; set; }
        [DefaultValue("Baykus Team")]
        public string Title { get; set; }
        [DefaultValue("İyi günler diler..")]
        public string Body { get; set; }
        
        [DefaultValue("https://cdn.pixabay.com/photo/2021/01/27/06/51/owl-5953875_1280.png")]
        public string ImageUrl { get; set; }
    }
    
    public class NotificationAllUserRequest
    {
        [DefaultValue("Baykus Team")]
        public string Title { get; set; }
        [DefaultValue("İyi günler diler..")]
        public string Body { get; set; }
        
        [DefaultValue("https://cdn.pixabay.com/photo/2021/01/27/06/51/owl-5953875_1280.png")]
        public string ImageUrl { get; set; }
    }
    
    public class UpdateDeviceTokenRequest
    {
        
        public string DeviceToken { get; set; }
    }
}