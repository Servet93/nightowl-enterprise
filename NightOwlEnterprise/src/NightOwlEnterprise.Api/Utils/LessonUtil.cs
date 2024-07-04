using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Utils;

public static class LessonUtil
{
    public static Dictionary<Lesson, string> enumToName = new()
    {
        { Lesson.Turkish, "Türkçe" },
        { Lesson.History, "Tarih" },
        { Lesson.History_1, "Tarih 1" },
        { Lesson.History_2, "Tarih 2" },
        { Lesson.Geography, "Coğrafya" },
        { Lesson.Geography_1, "Coğrafya 1" },
        { Lesson.Geography_2, "Coğrafya 2" },
        { Lesson.Philosophy, "Felsefe" },
        { Lesson.Religion, "Din Kültürü" },
        { Lesson.Mathematics, "Matematik" },
        { Lesson.Geometry, "Geometri" },
        { Lesson.Physics, "Fizik" },
        { Lesson.Chemistry, "Kimya" },
        { Lesson.Biology, "Biyoloji" },
        { Lesson.Literature, "Edebiyat" },
        { Lesson.English, "İngilizce" },
    };
    
    public static List<Lesson> TytLesson = new ()
    {
        Lesson.Turkish,
        Lesson.History,
        Lesson.Geography,
        Lesson.Philosophy,
        Lesson.Religion,
        Lesson.Mathematics,
        Lesson.Geometry,
        Lesson.Physics,
        Lesson.Chemistry,
        Lesson.Biology,
    };
    
    public static List<Lesson> MfLesson = new ()
    {
        Lesson.Mathematics,
        Lesson.Geometry,
        Lesson.Physics,
        Lesson.Chemistry,
        Lesson.Biology,
    };
    
    public static List<Lesson> TmLesson = new ()
    {
        Lesson.Literature,
        Lesson.History_1,
        Lesson.Geography_1,
        Lesson.Mathematics,
        Lesson.Geometry,
    };
    
    public static List<Lesson> SozelLesson = new ()
    {
        Lesson.Literature,
        Lesson.History_1,
        Lesson.History_2,
        Lesson.Geography_1,
        Lesson.Geography_2,
        Lesson.Philosophy,
        Lesson.Religion,
    };
    
    public static List<Lesson> DilLesson = new ()
    {
        Lesson.English,
    };

    public static bool IsLessonValidForExamType(ExamType examType, Lesson lesson)
    {
        if (examType == ExamType.TYT)
        {
            return TytLesson.Contains(lesson);
        }
        else if (examType == ExamType.MF)
        {
            return MfLesson.Contains(lesson);
        }
        else if (examType == ExamType.TM)
        {
            return TmLesson.Contains(lesson);
        }
        else if (examType == ExamType.Sozel)
        {
            return SozelLesson.Contains(lesson);
        }
        else if (examType == ExamType.Dil)
        {
            return DilLesson.Contains(lesson);
        }

        return false;
    }

    public static string GetName(Lesson lesson) => enumToName.ContainsKey(lesson) ? enumToName[lesson] : String.Empty;
}