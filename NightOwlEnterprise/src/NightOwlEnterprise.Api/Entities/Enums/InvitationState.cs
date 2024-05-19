namespace NightOwlEnterprise.Api.Entities.Enums;

public enum InvitationState
{
    SpecifyHour, // Saat Belirle(Sadece Koç belirleyebiliyor)
    WaitingApprove,
    Approved, // Görüşülecek
    Cancelled, //İptal edildi
    Open, // Açık
    Done, //Tamamlandı
}