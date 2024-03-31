using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MongoDB.Driver;

namespace NightOwlEnterprise.Api.Endpoints.Students;

public static class Onboard
{
    public static void MapOnboard<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        var examTypes = new string[5] { "mf", "tm", "sozel", "dil", "tyt" };
        
        var gradeTypes = new string[5] { "9", "10", "11", "12", "mezun" };
        
        endpoints.MapPost("/onboard", async Task<Results<Ok, ValidationProblem, NotFound>>
            (StudentOnboardRequest request, ClaimsPrincipal claimsPrincipal, [FromServices] IServiceProvider sp) =>
        {
            var requestValidation = RequestValidation(request);

            if (requestValidation.IsFailure)
            {
                return requestValidation.Error.CreateValidationProblem();    
            }
            
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
            
            var mongoDatabase = sp.GetRequiredService<IMongoDatabase>();
            
            var onboardStudentCollection = mongoDatabase.GetCollection<OnboardStudent>("OnboardStudents");

            var userIdStr = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub).Value;

            Guid.TryParse(userIdStr, out var userId);
            
            await onboardStudentCollection.InsertOneAsync(new OnboardStudent()
            {
                UserId = userId,
                Data = request,
            });
            
            return TypedResults.Ok();
        }).RequireAuthorization();
        
        Result RequestValidation(StudentOnboardRequest request)
        {
            if (string.IsNullOrEmpty(request.Name))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidStudentName(request.Name));  
            }
            
            if (string.IsNullOrEmpty(request.Surname))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidStudentSurname(request.Surname));
            }
            
            if (string.IsNullOrEmpty(request.Email))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidStudentEmail(request.Email));
            }
            
            if (string.IsNullOrEmpty(request.Mobile))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidStudentMobile(request.Mobile));
            }
            
            if (string.IsNullOrEmpty(request.ExamType) || !examTypes.Contains(request.ExamType.ToLower()))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidExamType(request.ExamType));
            }
            
            if (string.IsNullOrEmpty(request.Grade) || !gradeTypes.Contains(request.Grade.ToLower()))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidGrade(request.Grade));
            }

            if (request.IsTryPracticeTYTExamBefore)
            {
                if (request.LastPracticeTYTExamPoints == null)
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidGrade(request.Grade));
                }
                
                //Anlam Bilgisi: (Max 30, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Semantics >= 0 && request.LastPracticeTYTExamPoints.Semantics <= 30))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidSemanticNet", "Anlam bilgisi neti",
                        request.LastPracticeTYTExamPoints.Semantics, 30, 0));
                }
                
                //Dil Bilgisi: (Max 10, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Grammar >= 0 && request.LastPracticeTYTExamPoints.Grammar <= 10))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidGrammarNet", "Dil bilgisi neti",
                        request.LastPracticeTYTExamPoints.Grammar, 10, 0));
                }
                
                //Matematik: (Max 30, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Mathematics >= 0 && request.LastPracticeTYTExamPoints.Mathematics <= 30))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidMathematicsNet", "Matematik neti",
                        request.LastPracticeTYTExamPoints.Mathematics, 30, 0));
                }
                
                //Geometri: (Max 10, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Geometry >= 0 && request.LastPracticeTYTExamPoints.Geometry <= 10))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidGeometryNet", "Geometri neti",
                        request.LastPracticeTYTExamPoints.Mathematics, 10, 0));
                }
                
                //Tarih: (Max 5, Min 0)
                if (!(request.LastPracticeTYTExamPoints.History >= 0 && request.LastPracticeTYTExamPoints.History <= 5))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidHistoryNet", "Tarih neti",
                        request.LastPracticeTYTExamPoints.History, 5, 0));
                }
                
                //Coğrafya: (Max 5, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Geography >= 0 && request.LastPracticeTYTExamPoints.Geography <= 5))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidGeographyNet", "Coğrafya neti",
                        request.LastPracticeTYTExamPoints.Geography, 5, 0));
                }
                
                //Felsefe: (Max 5, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Philosophy >= 0 && request.LastPracticeTYTExamPoints.Philosophy <= 5))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidPhilosophyNet", "Felsefe neti",
                        request.LastPracticeTYTExamPoints.Philosophy, 5, 0));
                }
                
                //Din: (Max 5, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Religion >= 0 && request.LastPracticeTYTExamPoints.Religion <= 5))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidReligionNet", "Din neti",
                        request.LastPracticeTYTExamPoints.Religion, 5, 0));
                }
                
                //Fizik: (Max 7, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Physics >= 0 && request.LastPracticeTYTExamPoints.Physics <= 7))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidPhysicsNet", "Fizik neti",
                        request.LastPracticeTYTExamPoints.Physics, 7, 0));
                }
                
                //Kimya: (Max 7, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Chemistry >= 0 && request.LastPracticeTYTExamPoints.Chemistry <= 7))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidChemistryNet", "Kimya neti",
                        request.LastPracticeTYTExamPoints.Chemistry, 7, 0));
                }
                
                //Biyoloji: (Max 6, Min 0)
                if (!(request.LastPracticeTYTExamPoints.Biology >= 0 && request.LastPracticeTYTExamPoints.Biology <= 6))
                {
                    return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidBiologyNet", "Biyoloji neti",
                        request.LastPracticeTYTExamPoints.Biology, 6, 0));
                }
            }
            
            
            //F Part
            if (request.TYTGoalNet.HasValue && !(request.TYTGoalNet >= 0 && request.TYTGoalNet <= 120))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidTYTGoalNet", "TYT neti",
                    request.TYTGoalNet.Value, 120, 0));
            }
            
            if (request.AYTGoalNet.HasValue && !(request.AYTGoalNet >= 0 && request.AYTGoalNet <= 80))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidAYTGoalNet", "AYT neti",
                    request.AYTGoalNet.Value, 80, 0));
            }
            
            if (request.GoalRanking.HasValue && !(request.GoalRanking >= 1 && request.GoalRanking <= 5000000))
            {
                return Result.Failure(CommonErrorDescriptor.InvalidRange("InvalidGoalRanking", "Hedef sıralaması",
                    request.GoalRanking.Value, 5000000, 1));
            }
            
            //G Part
            if (request.SupplementaryMaterials == null)
            {
                
            }

            if (request.SupplementaryMaterials.PrivateTutoring)
            {
                if (request.SupplementaryMaterials.PrivateTutoringAYT)
                {
                    if (request.ExamType.ToLower() == "tm" && request.SupplementaryMaterials.PrivateTutoringAYTLessons.TM == null)
                    {
                        
                    }
                    
                    if (request.ExamType.ToLower() == "mf" && request.SupplementaryMaterials.PrivateTutoringAYTLessons.MF == null)
                    {
                        
                    }
                    
                    if ((request.ExamType.ToLower() == "sozel" || request.ExamType.ToLower() == "sözel") && request.SupplementaryMaterials.PrivateTutoringAYTLessons.Sozel == null)
                    {
                        
                    }
                    
                    if (request.ExamType.ToLower() == "dil" && request.SupplementaryMaterials.PrivateTutoringAYTLessons.Dil == null)
                    {
                        
                    }
                    
                }
            }
            
            return Result.Success();
        }
    }
    
    private static async Task<InfoResponse> CreateInfoResponseAsync<TUser>(TUser user, UserManager<TUser> userManager)
        where TUser : class
    {
        return new()
        {
            Email = await userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user),
        };
    }

    public class OnboardStudent
    {
        public Guid UserId { get; set; }
        
        public StudentOnboardRequest Data { get; set; }
    }
    
    public class StudentOnboardRequest
    {
        public string Name { get; set; }
        
        public string Surname { get; set; }
        
        public string Mobile { get; set; }
        
        public string Email { get; set; }
        
        //Alan -> MF,TM,Sözel,Dil,Tyt
        public string ExamType { get; set; }
        
        //Sınıf -> 9-10-11-12 ve Mezun
        public string Grade  { get; set; }
        
        public string ParentName { get; set; }
        
        public string ParentSurname { get; set; }
        
        public string ParentMobile { get; set; }

        public string ParentEmail { get; set; }
        
        //Okuduğunuz lise
        public string HighSchool { get; set; }
        
        //Lise orta öğretim başarı puanınız 0-100 arasında olmalı
        public float HighSchoolGPA { get; set; }

        //Daha önce TYT denemesine girdiniz mi? True ise LastPracticeTytExamPoints dolu olmalı. False ise 
        public bool IsTryPracticeTYTExamBefore { get; set; }
        
        public TYTAveragePoints LastPracticeTYTExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise
        //LastPracticeTytExamPoints dolu olmalı. False ise 
        public bool IsTryPracticeAYTExamBefore { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> MF
        public MFAveragePoints LastPracticeMFExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> TM
        public TMAveragePoints LastPracticeTMExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Sozel
        public SozelAveragePoints LastPracticeSozelExamPoints { get; set; }
        
        //Daha önce AYT denemesine girdiniz mi? True ise && ExamType -> Dil
        public DilAveragePoints LastPracticeDilExamPoints { get; set; }
        
        //TYT hedef netiniz: (Max 120, Min 0) (required değil)
        public byte? TYTGoalNet { get; set; }
        
        //AYT hedef netiniz: (Max 80, Min 0) (required değil)
        public byte? AYTGoalNet { get; set; }
        
        //Hedef sıralamanız: (1-SONSUZ rangeta integer alır) (required değil)
        public uint? GoalRanking { get; set; }
        
        //Hedef meslek/okul/bölümünüz: (Free text string alır) (required değil)
        public string DesiredProfessionSchoolField { get; set; }

        //Koçluktan beklentin nedir? (Free text uzun paragraf)
        public string ExpectationsFromCoaching { get; set; }

        public SupplementaryMaterials SupplementaryMaterials { get; set; }
    }

    public class TYTAveragePoints
    {
        //Anlam Bilgisi: (Max 30, Min 0)
        public byte Semantics { get; set; }
        //Dil Bilgisi: (Max 10, Min 0)
        public byte Grammar { get; set; }
        //Matematik: (Max 30, Min 0)
        public byte Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public byte Geometry { get; set; }
        //Tarih: (Max 5, Min 0)
        public byte History { get; set; }
        //Coğrafya: (Max 5, Min 0)
        public byte Geography { get; set; }
        //Felsefe: (Max 5, Min 0)
        public byte Philosophy { get; set; }
        //Din: (Max 5, Min 0)
        public byte Religion { get; set; }
        //Fizik: (Max 7, Min 0)
        public byte Physics { get; set; }
        //Kimya: (Max 7, Min 0)
        public byte Chemistry { get; set; }
        //Biology: (Max 6, Min 0)
        public byte Biology { get; set; }
    }
    
    public class MFAveragePoints
    {
        //Matematik: (Max 30, Min 0)
        public string Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public string Geometry { get; set; }
        //Fizik: (Max 14, Min 0)
        public string Physics { get; set; }
        //Kimya: (Max 13, Min 0)
        public string Chemistry { get; set; }
        //Biology: (Max 13, Min 0)
        public string Biology { get; set; }
    }
    
    public class TMAveragePoints
    {
        //Matematik: (Max 30, Min 0)
        public string Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public string Geometry { get; set; }
        //Edebiyat: (Max 24, Min 0)
        public string Literature { get; set; }
        //Tarih: (Max 10, Min 0)
        public string History { get; set; }
        //Coğrafya: (Max 6, Min 0)
        public string Geography { get; set; }
    }
    
    //Sözel
    public class SozelAveragePoints
    {
        //Tarih-1: (Max 10, Min 0)
        public string History1 { get; set; }
        //Coğrafya: (Max 24, Min 0)
        public string Geography1 { get; set; }
        //Edebiyat-1: (Max 6, Min 0)
        public string Literature1 { get; set; }
        //Tarih-2: (Max 11, Min 0)
        public string History2 { get; set; }
        //Coğrafya-2: (Max 11, Min 0)
        public string Geography2 { get; set; }
        //Felsefe: (Max 12, Min 0)
        public string Philosophy { get; set; }
        //Din: (Max 6, Min 0)
        public string Religion { get; set; }
    }
    
    public class DilAveragePoints
    {
        //YDT: (Max 80, Min 0)
        public string YTD { get; set; }
    }

    public enum ExamType
    {
        MF,
        TM,
        Sozel,
        Dil,
        Tyt,
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
        
        public bool PrivateTutoringTYT { get; set; }
        public PrivateTutoringTYT PrivateTutoringTYTLessons { get; set; }
        
        public bool PrivateTutoringAYT { get; set; }
        //AYT(Başlık):
        //Alanına göre a partında seçtiği dersler gelir. Hangi alandan hangi derslerin geleceğini e partındaki netler kısmından ulaşabilirsiniz.
        public PrivateTutoringAYT PrivateTutoringAYTLessons { get; set; }
        
        public bool Course { get; set; }
        public bool Youtube { get; set; }
    }

    public class PrivateTutoringTYT
    {
        public bool Turkish { get; set; }
        public bool Mathematics { get; set; }
        public bool Geometry { get; set; }
        public bool History { get; set; }
        public bool Geography { get; set; }
        public bool Philosophy { get; set; }
        public bool Religion { get; set; }
        public bool Physics { get; set; }
        public bool Chemistry { get; set; }
        public bool Biology { get; set; }
    }
    
    public class PrivateTutoringAYT
    {
        public MF MF { get; set; }
        public TM TM { get; set; }
        public Sozel Sozel { get; set; }
        public Dil Dil { get; set; }
    }
    
    public class MF
    {
        public bool Mathematics { get; set; }
        public bool Geometry { get; set; }
        public bool Physics { get; set; }
        public bool Chemistry { get; set; }
        public bool Biology { get; set; }
    }
    
    public class TM
    {
        //Matematik: (Max 30, Min 0)
        public bool Mathematics { get; set; }
        //Geometri: (Max 10, Min 0)
        public bool Geometry { get; set; }
        //Edebiyat: (Max 24, Min 0)
        public bool Literature { get; set; }
        //Tarih: (Max 10, Min 0)
        public bool History { get; set; }
        //Coğrafya: (Max 6, Min 0)
        public bool Geography { get; set; }
    }
    
    //Sözel
    public class Sozel
    {
        //Tarih-1: (Max 10, Min 0)
        public bool History1 { get; set; }
        //Coğrafya: (Max 24, Min 0)
        public bool Geography1 { get; set; }
        //Edebiyat-1: (Max 6, Min 0)
        public bool Literature1 { get; set; }
        //Tarih-2: (Max 11, Min 0)
        public bool History2 { get; set; }
        //Coğrafya-2: (Max 11, Min 0)
        public bool Geography2 { get; set; }
        //Felsefe: (Max 12, Min 0)
        public bool Philosophy { get; set; }
        //Din: (Max 6, Min 0)
        public bool Religion { get; set; }
    }
    
    public class Dil
    {
        //YDT: (Max 80, Min 0)
        public bool YTD { get; set; }
    }

    public enum SupplementaryMaterial
    {
        School,
        SpecialCourse,
        Course,
        Youtube
    }

    public static class CommonErrorDescriptor
    {
        public static ErrorDescriptor InvalidStudentName(string name)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidStudentName),
                Description = $"Öğrenci isim '{name}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidStudentSurname(string surname)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidStudentSurname),
                Description = $"Öğrenci soyisim '{surname}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidStudentMobile(string mobile)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidStudentMobile),
                Description = $"Öğrenci mobile '{mobile}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidStudentEmail(string email)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidStudentEmail),
                Description = $"Öğrenci email '{email}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidParentName(string name)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidParentName),
                Description = $"Veli isim '{name}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidParentSurname(string surname)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidParentSurname),
                Description = $"Veli soyisim '{surname}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidParentMobile(string mobile)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidParentMobile),
                Description = $"Veli mobile '{mobile}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidParentEmail(string email)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidParentEmail),
                Description = $"Veli email '{email}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidExamType(string examType)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidExamType),
                Description = $"Alan bilgisi '{examType}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidGrade(string grade)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidGrade),
                Description = $"Sınıf bilgisi '{grade}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidHighSchool(string highSchool)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidHighSchool),
                Description = $"Sınıf bilgisi '{highSchool}' geçersiz"
            };
        }
        
        public static ErrorDescriptor InvalidHighSchoolGPA(float highSchoolGpa)
        {
            return new ErrorDescriptor
            {
                Code = nameof(InvalidHighSchoolGPA),
                Description = $"Lise orta öğretim başarı puanı '{highSchoolGpa}' geçersiz.(0-100)"
            };
        }
        
        public static ErrorDescriptor InvalidRange(string code, string text, byte point, byte max, byte min)
        {
            return new ErrorDescriptor
            {
                Code = code,
                Description = $"{text} '{point}' geçersiz.({min}-{max})"
            };
        }
        
        public static ErrorDescriptor InvalidRange(string code, string text, uint point, uint max, uint min)
        {
            return new ErrorDescriptor
            {
                Code = code,
                Description = $"{text} '{point}' geçersiz.({min}-{max})"
            };
        }
    }
}