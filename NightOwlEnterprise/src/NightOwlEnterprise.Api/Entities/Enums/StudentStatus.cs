using System.ComponentModel;

namespace NightOwlEnterprise.Api.Entities.Enums;

public enum StudentStatus
{
    [Description("Payment Awaiting")]
    PaymentAwaiting,
    [Description("Onboard Progress")]
    OnboardProgress,
    [Description("Coach Select")]
    CoachSelect,
    [Description("Active")]
    Active,
}