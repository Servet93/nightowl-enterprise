namespace NightOwlEnterprise.Api.Entities;

public class UserDevice
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; }
    
    public string DeviceToken { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}