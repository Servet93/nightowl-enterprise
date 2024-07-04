using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Utils;

public class ProgramExamTypeUtil
{
    public static Dictionary<ProgramExamType, string> programExamTypeEnumToName = new()
    {
        { ProgramExamType.TYT , "TYT"},
        { ProgramExamType.AYT , "AYT"},
    };

    public static string GetName(ProgramExamType programExamType) =>
        programExamTypeEnumToName.ContainsKey(programExamType) ? programExamTypeEnumToName[programExamType] : string.Empty;

}