using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints;

public class SubscriptionHistory
{
    public int Id { get; set; } // Abonelik geçmişi kimliği

    public Guid UserId { get; set; } // Kullanıcı kimliği
    
    // ForeignKey ile ilişkiyi kurmak için ApplicationUser sınıfına referans
    public ApplicationUser User { get; set; }

    public DateTime SubscriptionStartDate { get; set; } // Abonelik başlangıç tarihi

    public DateTime? SubscriptionEndDate { get; set; } // Abonelik bitiş tarihi

    public SubscriptionType Type { get; set; } // Abonelik tipi (örneğin, ücretsiz, standart, premium)

    public string SubscriptionId { get; set; }
    
    public string InvoiceId { get; set; }
    
    public string SubscriptionState { get; set; }
    
    public string InvoiceState { get; set; }
    
    public string LastError { get; set; }
}