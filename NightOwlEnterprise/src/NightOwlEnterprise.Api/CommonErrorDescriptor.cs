namespace NightOwlEnterprise.Api;

 public static class CommonErrorDescriptor
    {
        public static ErrorDescriptor EmptyStudentGeneralInfo()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyStudentGeneralInfo),
                Description = $"Öğrenci bilgilerini giriniz"
            };
        }
        
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
        
        public static ErrorDescriptor EmptyParentInfo()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyParentInfo),
                Description = $"Veli bilgilerini giriniz"
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
        
        public static ErrorDescriptor EmptyAcademicSummary()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyHighSchool),
                Description = $"Akademiz özet bilgisi giriniz"
            };
        }
        
        public static ErrorDescriptor EmptyHighSchool()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyHighSchool),
                Description = $"Okuduğunuz lise bilgisini giriniz"
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
        
        public static ErrorDescriptor EmptyLastPracticeTYTExamPoints()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyLastPracticeTYTExamPoints),
                Description = $"Son girdiğiniz denemelerin netlerini giriniz!"
            };
        }
        
        public static ErrorDescriptor EmptyLastPracticeTMExamPoints()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyLastPracticeTMExamPoints),
                Description = $"Son girdiğiniz temel/matematik netlerini giriniz!"
            };
        }
        
        public static ErrorDescriptor EmptyLastPracticeMFExamPoints()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyLastPracticeTMExamPoints),
                Description = $"Son girdiğiniz matematik/fen netlerini giriniz!"
            };
        }
        
        public static ErrorDescriptor EmptyLastPracticeYDTExamPoints()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyLastPracticeYDTExamPoints),
                Description = $"Son girdiğiniz yabancı dil netlerini giriniz!"
            };
        }
        
        public static ErrorDescriptor EmptySupplementaryMaterials()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptySupplementaryMaterials),
                Description = $"Yardımcı kaynak bilgisini giriniz!"
            };
        }
        
        public static ErrorDescriptor EmptyTM()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyTM),
                Description = $"Temel/Matematik ders seçimi yapılmadı!"
            };
        }
        
        public static ErrorDescriptor EmptyMF()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyMF),
                Description = $"Matematik/Fen ders seçimi yapılmadı!"
            };
        }
        
        public static ErrorDescriptor EmptySozel()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptySozel),
                Description = $"Sözel ders seçimi yapılmadı!"
            };
        }
        
        public static ErrorDescriptor EmptyDil()
        {
            return new ErrorDescriptor
            {
                Code = nameof(EmptyDil),
                Description = $"Dil ders seçimi yapılmadı!"
            };
        }
        
        public static ErrorDescriptor RequiredExamTypes()
        {
            return new ErrorDescriptor
            {
                Code = nameof(RequiredExamTypes),
                Description = $"Alan bilgisi seçiniz!"
            };
        }
    }