using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Utils;

public class ExamTypeConverters
{
    public static Dictionary<ExamType, string> examToText = new Dictionary<ExamType, string>()
    {
        // { ExamType.TYT_TM, "TYT/Türkçe-Matematik" },
        // { ExamType.TYT_MF, "TYT/Matematik-Fen" },
        // { ExamType.TYT_SOZEL, "TYT/Sözel" },
        { ExamType.TYT, "TYT" },
        { ExamType.TM, "Türkçe-Matematik" },
        { ExamType.MF, "Matematik-Fen" },
        { ExamType.Sozel, "Sözel" },
        { ExamType.Dil, "Dil" },
        
    };

    public static string GetText(ExamType examType) => examToText.GetValueOrDefault(examType);
}