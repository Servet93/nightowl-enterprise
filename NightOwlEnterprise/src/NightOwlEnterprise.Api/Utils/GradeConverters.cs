using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Utils;

public class GradeConverters
{
    public static Dictionary<Grade, string> gradeToText = new Dictionary<Grade, string>()
    {
        { Grade.Dokuz, "9. Sınıf" },
        { Grade.On, "10. Sınıf" },
        { Grade.Onbir, "11. Sınıf" },
        { Grade.Oniki, "12. Sınıf" },
        { Grade.Mezun, "Mezun" },
    };

    public static string GetText(Grade grade) => gradeToText.GetValueOrDefault(grade);
}