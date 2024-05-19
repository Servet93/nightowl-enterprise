using System.Text.Json.Serialization;
using NightOwlEnterprise.Api.Entities.Enums;

namespace NightOwlEnterprise.Api.Endpoints.CommonDto;

public class StudentGeneralInfo
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
        
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExamType ExamType { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Grade Grade { get; set; }
}

public class ParentInfo
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
}

//Okuduğunuz lise
//Lise orta öğretim başarı puanınız 0-100 arasında olmalı
public class AcademicSummary
{
    public string HighSchool { get; set; }
    public float HighSchoolGPA { get; set; }
}

//TYT hedef netiniz: (Max 120, Min 0) (required değil)

//AYT hedef netiniz: (Max 80, Min 0) (required değil)

//Hedef sıralamanız: (1-SONSUZ rangeta integer alır) (required değil)

//Hedef meslek/okul/bölümünüz: (Free text string alır) (required değil)
public class StudentGoals
{
    public byte? TytGoalNet { get; set; }
    public byte? AytGoalNet { get; set; }
    public uint? GoalRanking { get; set; }
    public string? DesiredProfessionSchoolField { get; set; }   
}

public class SupplementaryMaterials
{
    public bool School { get; set; }
    //Özel Ders: Bunu işaretlerse hemen altına ek bir soru gelir:
    // Hangi derslerden özel ders alıyorsunuz?(Birden çok seçenek seçebilecek). Seçenekler:
    // TYT(Başlık):
    //Türkçe, Matematik, Geometri, Tarih, Coğrafya, Felsefe, Din, Fizik, Kimya, Biyoloji
    //AYT(Başlık):
    //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
    public bool PrivateTutoring { get; set; }
        
    public bool PrivateTutoringTyt { get; set; }
    public PrivateTutoringTYTObject? PrivateTutoringTytLessons { get; set; }
        
    public bool PrivateTutoringAyt { get; set; }
    //AYT(Başlık):
    //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
    public PrivateTutoringAYTObject? PrivateTutoringAytLessons { get; set; }
        
    public bool Course { get; set; }
    public bool Youtube { get; set; }
}