using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Utils;

public class TaskTypeUtil
{
    public static Dictionary<TaskType, string> taskTypeEnumToName = new()
    {
        { TaskType.Soru , "Soru/Test"},
        { TaskType.Konu , "Konu"},
        { TaskType.Deneme , "Deneme"},
        { TaskType.Tekrar , "Tekrar"},
    };

    public static string GetName(TaskType taskType) =>
        taskTypeEnumToName.ContainsKey(taskType) ? taskTypeEnumToName[taskType] : string.Empty;

}