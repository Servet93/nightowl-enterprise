using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NightOwlEnterprise.Api;
using NightOwlEnterprise.Api.Endpoints;
using NightOwlEnterprise.Api.Endpoints.Coachs;
using NightOwlEnterprise.Api.Endpoints.Invitations;
using NightOwlEnterprise.Api.Endpoints.Students;
using NightOwlEnterprise.Api.Endpoints.Universities;
using NightOwlEnterprise.Api.Services;
using NightOwlEnterprise.Api.Utils.Email;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using NLog.Layouts;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;
using JsonAttribute = NLog.Layouts.JsonAttribute;
using Onboard = NightOwlEnterprise.Api.Endpoints.Students.Onboard;

var builder = WebApplication.CreateBuilder(args);

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

var awsCloudWatchConfig = builder.Configuration.GetSection(AwsCloudWatchConfig.AwsCloudWatchConfigSection).Get<AwsCloudWatchConfig>();

if (awsCloudWatchConfig is not null && awsCloudWatchConfig.Enabled)
{
    var awsTarget = new AWSTarget()
    {
        LogGroup = awsCloudWatchConfig.LogGroup,
        Region = awsCloudWatchConfig.Region,
        Credentials = new BasicAWSCredentials(awsCloudWatchConfig.AccessKey,awsCloudWatchConfig.SecretKey),
        Layout = new JsonLayout()
        {
            IncludeGdc = true,
            IncludeEventProperties = true,
            IncludeScopeProperties = true,
            Attributes =
            {
                new JsonAttribute("timestamp", "${date:format=o}"),
                new JsonAttribute("level", "${level:upperCase=true}"),
                new JsonAttribute("logger", "${logger}"),
                new JsonAttribute("message", "${message}"),
                new JsonAttribute("exception", "${exception:format=ToString,Data}")
                {
                    IncludeEmptyValue = false
                },
                new JsonAttribute("stackTrace", "${stacktrace}")
                {
                    IncludeEmptyValue = false
                },
            }
        }
    };

    NLog.LogManager.Configuration.AddTarget("aws", awsTarget);
    NLog.LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Warn, awsTarget));

    NLog.LogManager.ReconfigExistingLoggers();    
}

logger.Fatal("Logger is created.");

builder.Host.UseNLog();

var isPostgresEnabled = builder.Configuration.GetValue<bool>("IsPostgresEnabled");

var postgresConnectionString = builder.Configuration.GetConnectionString("PostgresConnection");

try
{
    if (isPostgresEnabled)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (string.IsNullOrEmpty(postgresConnectionString))
            {
                logger.Fatal("ApplicationDbContext is using memory database.");
                options.UseInMemoryDatabase("AppDb");    
            }
            else
            {
                logger.Fatal("ApplicationDbContext is using postgresql.");
                options.UseNpgsql(postgresConnectionString);    
            }    
        });    
    }
}
catch (Exception e)
{
    logger.Error(e, "Postgres Connection Failed.");
    throw;
}

var isMongoEnabled = builder.Configuration.GetValue<bool>("IsMongoEnabled");
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoConnection");

if (isMongoEnabled && !string.IsNullOrEmpty(mongoConnectionString))
{
    // MongoDB istemci nesnesi oluşturma
    MongoClient mongoClient = new MongoClient(mongoConnectionString);

    // Veritabanı adı
    string databaseName = "chat";

    // Veritabanı nesnesi oluşturma
    IMongoDatabase database = mongoClient.GetDatabase(databaseName);

    var messasgesCollection = database.GetCollection<BsonDocument>("messages");

    // Azalan sıralama (descending) için index oluşturma
    var keysDescending = Builders<BsonDocument>.IndexKeys.Descending("timestamp");
    var indexOptionsDescending = new CreateIndexOptions { Name = "timestamp_-1" }; // "-1" azalan sıralamayı ifade eder
    await messasgesCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(keysDescending, indexOptionsDescending));

    var conversationIdDescending = Builders<BsonDocument>.IndexKeys.Ascending("conversationId");
    await messasgesCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(conversationIdDescending));
    
    // Veritabanı nesnesini dependency injection ile servislere ekleme
    builder.Services.AddSingleton(database);
}

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddSingleton<GetCoachAvailabilityDays>();

var stripeCredential = builder.Configuration.GetSection(StripeCredential.StripeSection).Get<StripeCredential>();

// This is a public sample test API key.
// Don’t submit any personally identifiable information in requests made with this key.
// Sign in to see your own test API key embedded in code samples.
// Stripe.StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";
// Stripe.StripeConfiguration.ApiKey = "sk_test_51OsO7cEyxtA03PfNAGZBvY40v3lzbZLF7Bb0BYOG8wRdlXnLhJoCXUIjtIOCyZtawn5lh97dnu6O0J5jcMxDL00O00WekY3Ta7";
if (stripeCredential is null || string.IsNullOrEmpty(stripeCredential.SecretKey))
{
    logger.Fatal("Stripe is not active.");    
}
else
{
    logger.Fatal("Stripe is active.");
    Stripe.StripeConfiguration.ApiKey = stripeCredential.SecretKey;    
}

builder.Services.Configure<StripeCredential>(
    builder.Configuration.GetSection(StripeCredential.StripeSection));

var jwtConfig = builder.Configuration.GetSection(JwtConfig.JwtSection).Get<JwtConfig>();

builder.Services.Configure<JwtConfig>(
    builder.Configuration.GetSection(JwtConfig.JwtSection));

if (jwtConfig is not null && !string.IsNullOrEmpty(jwtConfig.Key))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Key)),
        
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
        
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chatHub")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("Student", policy => policy.RequireRole("Student"));
        options.AddPolicy("Coach", policy => policy.RequireRole("Coach"));
        options.AddPolicy("Pdr", policy => policy.RequireRole("Pdr"));
    });    
}

var zoomCredential = new ZoomCredential();
builder.Configuration.GetSection(ZoomCredential.ZoomSection).Bind(zoomCredential);
builder.Services.AddSingleton<ZoomCredential>(zoomCredential);

builder.Services.AddSingleton<Zoom>();

builder.Services.AddScoped<ApplicationUserManager>();

builder.Services.AddSingleton<TurkishIdentityErrorDescriber>();

builder.Services.AddSingleton<StudentEmailSender>();

builder.Services.AddSingleton<JwtHelper>();

builder.Services.AddSingleton<LockManager>();

builder.Services.Configure<CoachConfig>(
    builder.Configuration.GetSection(CoachConfig.CoachSection));

builder.Services.AddSignalR();

var smtpServerCredential = builder.Configuration.GetSection(SmtpServerCredential.SmtpServerSection).Get<SmtpServerCredential>();

if (smtpServerCredential!.Enabled)
{
    builder.Services.Configure<SmtpServerCredential>(
        builder.Configuration.GetSection(SmtpServerCredential.SmtpServerSection));
    builder.Services.AddTransient(typeof(IEmailSender<ApplicationUser>), typeof(StudentIdentityEmailSender));
    builder.Services.AddTransient<IEmailSender, ProductionEmailSender>();    
}
else
{
    // builder.Services.TryAddTransient(typeof(IEmailSender<>), typeof(DefaultMessageEmailSender<>));
    // builder.Services.TryAddTransient<IEmailSender, NoOpEmailSender>();
    builder.Services.AddTransient<IEmailSender, LocalEmailSender>();    
}

var requireConfirmedEmail = builder.Configuration.GetValue<bool>("RequireConfirmedEmail");

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;

    // options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
    
    options.Password.RequireDigit = false;       // En az bir rakam içermeli
    options.Password.RequireLowercase = false;   // En az bir küçük harf içermeli
    options.Password.RequireUppercase = false;   // En az bir büyük harf içermeli
    options.Password.RequireNonAlphanumeric = false; // En az bir özel karakter içermeli
    options.Password.RequiredLength = 8;        // Minimum şifre uzunluğu
    options.Password.RequiredUniqueChars = 3;    // Şifrede kaç farklı karakter olmalı
    
    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(swaggerGenOptions => 
{
    swaggerGenOptions.ExampleFilters();
    
    // Endpoint gruplaması için
    swaggerGenOptions.TagActionsBy(api => api.GroupName);
    
    // Identity Bearer Authentication şemasını ekleyin
    swaggerGenOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Please enter a valid access token",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    // Swagger UI üzerinde "Authorize" butonunu gösterin
    swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    swaggerGenOptions.MapType<TimeSpan>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("00:00:00.000")
    });
    
    swaggerGenOptions.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new OpenApiDate(DateTime.Now.Date)
    }); // Tarih formatını belirt
});

//Swagger örneklerini nerede arayacağını belirtiyoruz
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddProblemDetails();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<PaginationUriBuilder>(o =>
{
    var accessor = o.GetRequiredService<IHttpContextAccessor>();
    var request = accessor.HttpContext?.Request;
    var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
    return new PaginationUriBuilder(uri);
});

var app = builder.Build();

logger.Fatal("App is created.");

if (isPostgresEnabled && !string.IsNullOrEmpty(postgresConnectionString))
{
    logger.Fatal("Db migration is started");
    
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
    }
    catch (Exception e)
    {
        logger.Error(e, "Postgres Migration Failed.");
        throw;
    }

    logger.Fatal("Db migration is finished");    
}

if (isPostgresEnabled)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    SeedDepartmentsAndUniversities(db);
    await SeedCoachs(db, userManager);
}

app.UseCors("AllowAll");

app.UseStaticFiles();

app.UseExceptionHandler();

app.UseStatusCodePages();

// app.UseExceptionHandler(exceptionHandlerApp =>
// {
//     exceptionHandlerApp.Run(async context => await Results.Problem().ExecuteAsync(context));
// });

// app.UseStatusCodePages(async statusCodeContext 
//     => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
//         .ExecuteAsync(statusCodeContext.HttpContext));



app.UseSwagger();
app.UseSwaggerUI();
// app.UseSwaggerUI(swaggerUiOptions =>
// {
//     swaggerUiOptions.
//     // Swagger UI üzerinde "Authorize" butonunu ekleyin
//     swaggerUiOptions.AddJwtBearer("Bearer", options =>
//     {
//         options.Description = "JWT Authorization header using the Bearer scheme.";
//         options.Type = "apiKey";
//         options.In = "header";
//         options.Name = "Authorization";
//     });
// });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (isMongoEnabled && !string.IsNullOrEmpty(mongoConnectionString))
{
    app.MapHub<ChatHub>("/chatHub", options =>
    {
        options.Transports = HttpTransportType.WebSockets;
    });    
}
else
{
    app.MapGet("/chatHub", async context =>
    {
        await context.Response.WriteAsync("chat hub not configured.");
    });
}

app.MapGet("/", async context =>
{
    context.Response.ContentType = "text/html";

    await context.Response.WriteAsync(@"
        <!DOCTYPE html>
        <html lang=""en"">
        <head>
            <meta charset=""UTF-8"">
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
            <title>Hello, World!</title>
            <style>
                body {
                    font-family: 'Arial', sans-serif;
                    background-color: #ffffff;
                    text-align: center;
                    margin: 50px;
                }
                h1 {
                    color: #333;
                }
                .logo {
                    width: 30%;
                    height: auto;
                    margin-bottom: 20px;
                }
            </style>
        </head>
        <body>
            <img src=""https://cdn.pixabay.com/photo/2021/01/27/06/51/owl-5953875_1280.png"" alt=""Logo"" class=""logo"">
            <h1>Hello, Guys!</h1>
        </body>
        </html>
    ");
});
//
// app.MapGet("/by", () => "Servet SEKER");
//
// app.MapGet("/exception", () =>
// {
//     var t = Results.Ok(new User(2));
//     var y = TypedResults.Ok(new User(2));
//     throw new Exception("Custom Exception by Servet SEKER");
// });
//
// app.MapGet("/users/{id:int}", (int id) 
//     => id <= 0 ? Results.BadRequest() : Results.Ok(new User(id)) );
//
// app.MapGet("/todos/{id}", Results<Ok<User>, NotFound>(int id) =>
//     id < 0 ? TypedResults.Ok(new User(id)) : TypedResults.NotFound());

app.MapCommonIdentityApi();
app.MapStudentsIdentityApi<ApplicationUser>();
app.MapCoachsIdentityApi<ApplicationUser>();
app.MapInvitationsApi();
app.MapUniversityApi();

app.MapGet("/conf", async context =>
{
    // ILoggerFactory örneği al
    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
    
    // ILogger örneğini oluştur
    var logger = loggerFactory.CreateLogger("ConfigEndpoint");
    
    logger.LogInformation("Aws Cloud Watch Conf");
    
    StringBuilder sb = new();
    sb.AppendLine($"IsMongoEnabled -> {isMongoEnabled}");
    sb.AppendLine($"Mongo -> {mongoConnectionString}");
    sb.AppendLine($"IsPostgresEnabled -> {isPostgresEnabled}");
    sb.AppendLine($"Postgres -> {postgresConnectionString}");
    sb.AppendLine($"Stripe.PublishableKey -> {stripeCredential.PublishableKey}");
    sb.AppendLine($"Stripe.SecretKey -> {stripeCredential.SecretKey}");
    sb.AppendLine($"Stripe.SigningSecret -> {stripeCredential.SigningSecret}");
    sb.AppendLine($"Stripe.DereceliKocPriceId -> {stripeCredential.DereceliKocPriceId}");
    sb.AppendLine($"Stripe.PdrPriceId -> {stripeCredential.PdrPriceId}");
    sb.AppendLine($"JwtConfig.Audience -> {jwtConfig!.Audience}");
    sb.AppendLine($"JwtConfig.Issuer -> {jwtConfig.Issuer}");
    sb.AppendLine($"JwtConfig.Key -> {jwtConfig.Key}");
    sb.AppendLine($"SmtpServerCredential.Enabled -> {smtpServerCredential.Enabled}");
    sb.AppendLine($"SmtpServerCredential.Address -> {smtpServerCredential.Address}");
    sb.AppendLine($"SmtpServerCredential.Port -> {smtpServerCredential.Port}");
    sb.AppendLine($"SmtpServerCredential.Username -> {smtpServerCredential.Username}");
    sb.AppendLine($"SmtpServerCredential.Password -> {smtpServerCredential.Password}");
    sb.AppendLine($"SmtpServerCredential.DisplayName -> {smtpServerCredential.DisplayName}");
    sb.AppendLine($"SmtpServerCredential.EnableSsl -> {smtpServerCredential.EnableSsl}");
    await context.Response.WriteAsync(sb.ToString());
});


app.MapGet("/zmeet", async context =>
{
    // var t = new Zoom();
    //
    // var clientId = "HG01lnYcQam6B_JzmvAPDg";
    // var clientSecret = "6Sw3xtFj2KIB2b4LWLzBPpAGZjKbOhv9";
    //
    // var p = await t.GetZoomAccessTokenAsync(clientId, clientSecret);
    //
    // await t.GetUserInfoAsync(p);
    //
    // var c = await t.CreateMeetingAsync(p, "17p2DfQZRgypCNXy4WG9nQ", "Baykus Görüşme", DateTime.UtcNow.AddMinutes(5), 60);
    //
    // await t.AddRegistrantAsync(p, c, "invictisec@gmail.com", "Invicti");
    // await t.AddRegistrantAsync(p, c, "servetseker93@gmail.com", "Servet");
    // await t.AddRegistrantAsync(p, c, "burakalparsln@gmail.com", "Burak");
    
    await context.Response.WriteAsync("Zoom Meet is running");
});

app.Run();

logger.Fatal("App is running.");

void SeedDepartmentsAndUniversities(ApplicationDbContext dbContext)
{
    var hasAnyDepartment = dbContext.Departments.Any();

    var departments = new List<Department>
    {
        new Department { Id = CommonVariables.ComputerEngineeringDepartmentId, Name = "Bilgisayar Mühendisliği", DepartmentType = DepartmentType.MF },
        new Department { Id = CommonVariables.ElectricalAndElectronicsEngineeringDepartmentId, Name = "Elektrik-Elektronik Mühendisliği", DepartmentType = DepartmentType.MF },
        new Department { Id = CommonVariables.MechanicalEngineeringDepartmentId, Name = "Makine Mühendisliği" , DepartmentType = DepartmentType.MF },
        new Department { Id = CommonVariables.IndustrialEngineeringDepartmentId, Name = "Endüstri Mühendisliği", DepartmentType = DepartmentType.MF },
        new Department { Id = CommonVariables.SociologyDepartmentId, Name = "Sosyoloji", DepartmentType = DepartmentType.TM },
        new Department { Id = CommonVariables.HistoryDepartmentId, Name = "Tarih", DepartmentType = DepartmentType.Sozel },
        new Department { Id = CommonVariables.PsychologyDepartmentId, Name = "Psikoloji", DepartmentType = DepartmentType.TM },
        new Department { Id = CommonVariables.GazetecilikDepartmentId, Name = "Gazetecilik", DepartmentType = DepartmentType.Sozel },
        new Department { Id = CommonVariables.ReklamcilikDepartmentId, Name = "Reklamcılık", DepartmentType = DepartmentType.Sozel },
        new Department { Id = CommonVariables.IsletmeDepartmentId, Name = "İşletme", DepartmentType = DepartmentType.TM },
        new Department { Id = CommonVariables.EnglishLanguageAndLiteratureDepartmentId, Name = "İngiliz Dili ve Edebiyatı", DepartmentType = DepartmentType.Dil },
        new Department { Id = CommonVariables.FrenchLanguageAndLiteratureDepartmentId, Name = "Fransız Dili ve Edebiyatı", DepartmentType = DepartmentType.Dil },
        new Department { Id = CommonVariables.GermanLanguageAndLiteratureDepartmentId, Name = "Alman Dili ve Edebiyatı", DepartmentType = DepartmentType.Dil},
        // Diğer departmanları buraya ekleyin
    };
    
    if (!hasAnyDepartment)
    {
        dbContext.Departments.AddRange(departments);
        dbContext.SaveChanges();
    }

    var hasAnyUniversity = dbContext.Universities.Any();

    var universities = new List<University>
    {
        new University
        {
            Id = CommonVariables.BosphorusUniversityId, Name = "Boğaziçi Üniversitesi",
        },
        new University
        {
            Id = CommonVariables.MiddleEastTechnicalUniversityId, Name = "Orta Doğu Teknik Üniversitesi",
        },
        new University
        {
            Id = CommonVariables.IstanbulTechnicalUniversityId, Name = "İstanbul Teknik Üniversitesi",
        },
        // Diğer üniversiteleri buraya ekleyin
    };
    
    if (!hasAnyUniversity)
    {
        dbContext.Universities.AddRange(universities);
        dbContext.SaveChanges();
    }

    var hasAnyUniversityDepartment = dbContext.UniversityDepartments.Any();

    if (!hasAnyUniversityDepartment)
    {
        var _universities = dbContext.Universities.ToList();
        var _departments = dbContext.Departments.ToList();
        
        foreach (var university in _universities)
        {
            foreach (var department in _departments)
            {
                university.UniversityDepartments.Add(new UniversityDepartment()
                {
                    UniversityId = university.Id,
                    DepartmentId = department.Id
                });
            }
        }

        dbContext.SaveChanges();
    }

    /*
     *             UniversityDepartments = departments.Select(d => new UniversityDepartment
            {
                UniversityId = Guid.NewGuid(),
                DepartmentId = d.Id,
                Department = d
            }).ToList()
     */

}

async Task SeedCoachs(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
{
    var hasAnyCoach = dbContext.Users.Any(x => x.UserType == UserType.Coach);
    
    var rnd = new Random();

    var index = 0;

    var namesAndGender = CommonVariables.NamesAndGender.DistinctBy(x => x.Item1).ToList();
    
    if (!hasAnyCoach)
    {
        var universities = dbContext.Universities.ToList();

        var departments = dbContext.Departments.ToList();

        // universities * departments(39) * 5 = 195
        var persons = PersonGenerator.GeneratePeople(195);

        var counter = 0;
        
        foreach (var university in universities)
        {
            foreach (var department in departments)
            {
                for (int i = 0; i < 5; i++)
                {
                    var x = (byte)rnd.Next(5);
                    
                    var mondayQuota = x == 0 ? (byte)1 : (byte)rnd.Next(5);
                    var tuesdayQuota = x == 1 ? (byte)1 : (byte)rnd.Next(5);
                    var wednesdayQuota = x == 2 ? (byte)1 : (byte)rnd.Next(5);
                    var thursayQuota = x == 3 ? (byte)1 : (byte)rnd.Next(5);
                    var fridayQuota = x == 4 ? (byte)1 : (byte)rnd.Next(5);

                    var studentQuota = (byte)(mondayQuota + tuesdayQuota + wednesdayQuota + thursayQuota + fridayQuota);
                    
                    var applicationUser = new ApplicationUser()
                    {
                        Id = persons[counter].Id,
                        Name = persons[counter].FirstName +" "+ persons[counter].LastName,
                        UserName = persons[counter].FirstName + persons[counter].LastName,
                        Email = persons[counter].Email,
                        PhoneNumber = persons[counter].Phone,
                        City = persons[counter].City,
                        Address = persons[counter].Address,
                        CoachDetail = new CoachDetail()
                        {
                            Name = persons[counter].FirstName,
                            Surname = persons[counter].LastName,
                            Email = persons[counter].Email,
                            Male = persons[counter].Gender,
                            BirthDate = DateTime.Now,
                            DepartmentType = department.DepartmentType,
                            Mobile = persons[counter].Phone,
                            UniversityId = university.Id,
                            DepartmentId = department.Id,
                            FirstTytNet = (byte)rnd.Next(90, 120),
                            LastTytNet = (byte)rnd.Next(90, 120),
                            FirstAytNet = (byte)rnd.Next(60, 80),
                            LastAytNet = (byte)rnd.Next(60, 80),
                            School = rnd.Next(100) % 2 == 0,
                            UsedYoutube = rnd.Next(100) % 2 == 0,
                            GoneCramSchool = rnd.Next(100) % 2 == 0,
                            Rank = (uint)rnd.Next(5000),
                            IsGraduated = rnd.Next(6) != 1 ? true : false,
                            HighSchool = persons[counter].HighSchoolName,
                            HighSchoolGPA = persons[counter].HighSchoolScore,
                            StudentQuota = studentQuota,
                            MondayQuota = mondayQuota,
                            TuesdayQuota = tuesdayQuota,
                            WednesdayQuota = wednesdayQuota,
                            ThursdayQuota = thursayQuota,
                            FridayQuota = fridayQuota,
                            PrivateTutoring = true,
                        },
                        TytNets = new TYTNets()
                        {
                            Biology = (byte)rnd.Next(6),
                            Chemistry = (byte)rnd.Next(7),
                            Geometry = (byte)rnd.Next(10),
                            Physics = (byte)rnd.Next(7),
                            Mathematics = (byte)rnd.Next(30),
                            Geography = (byte)rnd.Next(5),
                            Grammar = (byte)rnd.Next(10),
                            History = (byte)rnd.Next(5),
                            Philosophy = (byte)rnd.Next(5),
                            Religion = (byte)rnd.Next(5),
                            Semantics = (byte)rnd.Next(30),
                        },
                        UserType = UserType.Coach,
                    };

                    for (int j = 2016; j < 2024; j++)
                    {
                        var isEntered = rnd.Next(10) % 2 == 0;
                        var rank = isEntered ? rnd.Next(5000) : 0;
                        applicationUser.CoachYksRankings.Add(new CoachYksRanking()
                            { Year = j.ToString(), Enter = isEntered, Rank = (uint)rank });
                    }
                    
                    counter += 1;
                    
                    if (department.DepartmentType == DepartmentType.MF)
                    {
                        applicationUser.MfNets = new MFNets()
                        {
                            Biology = (byte)rnd.Next(13),
                            Chemistry = (byte)rnd.Next(13),
                            Geometry = (byte)rnd.Next(10),
                            Physics = (byte)rnd.Next(14),
                            Mathematics = (byte)rnd.Next(30),
                        };

                        applicationUser.PrivateTutoringMF = new PrivateTutoringMF()
                        {
                            Biology = rnd.Next(2) % 2 == 0,
                            Chemistry = rnd.Next(2) % 2 == 0,
                            Geometry = rnd.Next(2) % 2 == 0,
                            Mathematics = rnd.Next(2) % 2 == 0,
                            Physics = rnd.Next(2) % 2 == 0,
                        };
                        
                    }
                    else if (department.DepartmentType == DepartmentType.TM)
                    {
                        applicationUser.TmNets = new TMNets()
                        {
                            Geography = (byte)rnd.Next(6),
                            History = (byte)rnd.Next(10),
                            Geometry = (byte)rnd.Next(10),
                            Literature = (byte)rnd.Next(24),
                            Mathematics = (byte)rnd.Next(30),
                        };
                        
                        applicationUser.PrivateTutoringTM = new PrivateTutoringTM()
                        {
                            Geography = rnd.Next(2) % 2 == 0,
                            Geometry = rnd.Next(2) % 2 == 0,
                            History = rnd.Next(2) % 2 == 0,
                            Mathematics = rnd.Next(2) % 2 == 0,
                            Literature = rnd.Next(2) % 2 == 0,
                        };
                    }
                    else if (department.DepartmentType == DepartmentType.Sozel)
                    {
                        applicationUser.SozelNets = new SozelNets()
                        {
                            Geography1 = (byte)rnd.Next(24),
                            Geography2 = (byte)rnd.Next(11),
                            History1 = (byte)rnd.Next(10),
                            History2 = (byte)rnd.Next(11),
                            Literature1 = (byte)rnd.Next(6),
                            Philosophy = (byte)rnd.Next(12),
                            Religion = (byte)rnd.Next(6),
                        };
                        
                        applicationUser.PrivateTutoringSozel = new PrivateTutoringSozel()
                        {
                            Geography1 = rnd.Next(2) % 2 == 0,
                            Geography2 = rnd.Next(2) % 2 == 0,
                            History1 = rnd.Next(2) % 2 == 0,
                            History2 = rnd.Next(2) % 2 == 0,
                            Literature1 = rnd.Next(2) % 2 == 0,
                            Philosophy = rnd.Next(2) % 2 == 0,
                            Religion = rnd.Next(2) % 2 == 0,
                        };
                        
                    }
                    else if (department.DepartmentType == DepartmentType.Dil)
                    {
                        applicationUser.DilNets = new DilNets()
                        {
                            YDT = (byte)rnd.Next(80),
                        };                  
                        
                        applicationUser.PrivateTutoringDil = new PrivateTutoringDil()
                        {
                            YTD = rnd.Next(2) % 2 == 0,
                        };
                    }
                    
                    try
                    {
                        await userManager.CreateAsync(applicationUser, "Aa123456");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}

public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<University> Universities { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<UniversityDepartment> UniversityDepartments { get; set; }
    public DbSet<CoachDetail> CoachDetail { get; set; }
    public DbSet<CoachYksRanking> CoachYksRankings { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    
    public DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }
    
    public DbSet<PrivateTutoringTYT> PrivateTutoringTYT { get; set; }
    public DbSet<PrivateTutoringMF> PrivateTutoringMF { get; set; }
    public DbSet<PrivateTutoringTM> PrivateTutoringTM { get; set; }
    public DbSet<PrivateTutoringSozel> PrivateTutoringSozel { get; set; }
    public DbSet<PrivateTutoringDil> PrivateTutoringDil { get; set; }
    public DbSet<CoachStudentTrainingSchedule> CoachStudentTrainingSchedules { get; set; }
    
    public DbSet<ZoomMeetDetail> ZoomMeetDetails { get; set; }

    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");
        
        // ZoomMeetDetail ile Invitation arasındaki birebir ilişkiyi belirtme
        builder.Entity<ZoomMeetDetail>()
            .HasOne(z => z.Invitation)
            .WithOne(i => i.ZoomMeetDetail)
            .HasForeignKey<ZoomMeetDetail>(z => z.InvitationId);
        
        builder.Entity<Invitation>()
            .HasOne(z => z.ZoomMeetDetail)
            .WithOne(i => i.Invitation)
            .HasForeignKey<Invitation>(z => z.ZoomMeetDetailId);

        // ApplicationUser ile CoachStudentTrainingSchedule arasındaki ilişkiyi belirtme
        builder.Entity<CoachStudentTrainingSchedule>()
            .HasOne(csts => csts.Coach)
            .WithMany(u => u.CoachStudentTrainingSchedules)
            .HasForeignKey(csts => csts.CoachId);

        builder.Entity<CoachStudentTrainingSchedule>()
            .HasOne(csts => csts.Student)
            .WithMany(u => u.StudentCoachTrainingSchedules)
            .HasForeignKey(csts => csts.StudentId);
        
        // Invitation ile Coach arasında ilişki
        builder.Entity<Invitation>()
            .HasOne(c => c.Coach)
            .WithMany(u => u.InvitationsAsCoach)
            .HasForeignKey(c => c.CoachId);

        // Invitation ile Student arasında ilişki
        builder.Entity<Invitation>()
            .HasOne(c => c.Student)
            .WithMany(u => u.InvitationsAsStudent)
            .HasForeignKey(c => c.StudentId);
        
        builder.Entity<Invitation>()
            .Property(cc => cc.Date)
            .HasColumnType("date");

        // CoachDetail tablosunun ilişkilerini belirtiyoruz
        builder.Entity<CoachDetail>()
            .HasKey(cd => cd.CoachId); // Primary key'i belirtiyoruz
        
        // CoachDetail ile ApplicationUser arasında ilişki
        builder.Entity<CoachDetail>()
            .HasOne(cd => cd.Coach)
            .WithOne(u => u.CoachDetail)
            .HasForeignKey<CoachDetail>(cd => cd.CoachId);
        
        builder.Entity<CoachDetail>()
            .Property(cc => cc.BirthDate)
            .HasColumnType("date");
        
        // CoachDetail tablosunun ilişkilerini belirtiyoruz
        builder.Entity<StudentDetail>()
            .HasKey(cd => cd.StudentId); // Primary key'i belirtiyoruz

        // CoachDetail ile ApplicationUser arasında ilişki
        builder.Entity<StudentDetail>()
            .HasOne(cd => cd.Student)
            .WithOne(u => u.StudentDetail)
            .HasForeignKey<StudentDetail>(cd => cd.StudentId);
        
        // PrivateTutoringTYT tablosunun ilişkilerini belirtiyoruz
        builder.Entity<PrivateTutoringTYT>()
            .HasKey(cd => cd.CoachId); // Primary key'i belirtiyoruz
        
        // PrivateTutoringTYT ile ApplicationUser arasında ilişki
        builder.Entity<PrivateTutoringTYT>()
            .HasOne(cd => cd.Coach)
            .WithOne(u => u.PrivateTutoringTYT)
            .HasForeignKey<PrivateTutoringTYT>(cd => cd.CoachId);
        
        // PrivateTutoringMF tablosunun ilişkilerini belirtiyoruz
        builder.Entity<PrivateTutoringMF>()
            .HasKey(cd => cd.CoachId); // Primary key'i belirtiyoruz
        
        // PrivateTutoringMF ile ApplicationUser arasında ilişki
        builder.Entity<PrivateTutoringMF>()
            .HasOne(cd => cd.Coach)
            .WithOne(u => u.PrivateTutoringMF)
            .HasForeignKey<PrivateTutoringMF>(cd => cd.CoachId);
        
        // PrivateTutoringTM tablosunun ilişkilerini belirtiyoruz
        builder.Entity<PrivateTutoringTM>()
            .HasKey(cd => cd.CoachId); // Primary key'i belirtiyoruz
        
        // PrivateTutoringTM ile ApplicationUser arasında ilişki
        builder.Entity<PrivateTutoringTM>()
            .HasOne(cd => cd.Coach)
            .WithOne(u => u.PrivateTutoringTM)
            .HasForeignKey<PrivateTutoringTM>(cd => cd.CoachId);
        
        // PrivateTutoringSozel tablosunun ilişkilerini belirtiyoruz
        builder.Entity<PrivateTutoringSozel>()
            .HasKey(cd => cd.CoachId); // Primary key'i belirtiyoruz
        
        // PrivateTutoringSozel ile ApplicationUser arasında ilişki
        builder.Entity<PrivateTutoringSozel>()
            .HasOne(cd => cd.Coach)
            .WithOne(u => u.PrivateTutoringSozel)
            .HasForeignKey<PrivateTutoringSozel>(cd => cd.CoachId);
        
        // PrivateTutoringDil tablosunun ilişkilerini belirtiyoruz
        builder.Entity<PrivateTutoringDil>()
            .HasKey(cd => cd.CoachId); // Primary key'i belirtiyoruz
        
        // PrivateTutoringDil ile ApplicationUser arasında ilişki
        builder.Entity<PrivateTutoringDil>()
            .HasOne(cd => cd.Coach)
            .WithOne(u => u.PrivateTutoringDil)
            .HasForeignKey<PrivateTutoringDil>(cd => cd.CoachId);
        
        // CoachTYTNets tablosunun ilişkilerini belirtiyoruz
        builder.Entity<TYTNets>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // CoachTYTNets ile ApplicationUser arasında ilişki
        builder.Entity<TYTNets>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.TytNets)
            .HasForeignKey<TYTNets>(cd => cd.UserId);
        
        // CoachTMNets tablosunun ilişkilerini belirtiyoruz
        builder.Entity<TMNets>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // CoachTMNets ile ApplicationUser arasında ilişki
        builder.Entity<TMNets>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.TmNets)
            .HasForeignKey<TMNets>(cd => cd.UserId);
        
        // CoachMFNets tablosunun ilişkilerini belirtiyoruz
        builder.Entity<MFNets>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // CoachMFNets ile ApplicationUser arasında ilişki
        builder.Entity<MFNets>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.MfNets)
            .HasForeignKey<MFNets>(cd => cd.UserId);
        
        // CoachSozelNets tablosunun ilişkilerini belirtiyoruz
        builder.Entity<SozelNets>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // CoachSozelNets ile ApplicationUser arasında ilişki
        builder.Entity<SozelNets>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.SozelNets)
            .HasForeignKey<SozelNets>(cd => cd.UserId);
        
        // CoachDilNets tablosunun ilişkilerini belirtiyoruz
        builder.Entity<DilNets>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // CoachDilNets ile ApplicationUser arasında ilişki
        builder.Entity<DilNets>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.DilNets)
            .HasForeignKey<DilNets>(cd => cd.UserId);

        builder.Entity<UniversityDepartment>()
            .HasKey(ud => new { ud.UniversityId, ud.DepartmentId });

        builder.Entity<UniversityDepartment>()
            .HasOne(ud => ud.University)
            .WithMany(u => u.UniversityDepartments)
            .HasForeignKey(ud => ud.UniversityId);

        builder.Entity<UniversityDepartment>()
            .HasOne(ud => ud.Department)
            .WithMany(d => d.UniversityDepartments)
            .HasForeignKey(ud => ud.DepartmentId);
    }
}

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser<Guid>
{
    public string Name { get; set; } = String.Empty;
    public string Address { get; set; }  = String.Empty;
    public string City { get; set; }  = String.Empty;
    
    public UserType UserType { get; set; }
    
    public string CustomerId { get; set; } = String.Empty;
    
    public string SubscriptionId { get; set; } = String.Empty;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiration { get; set; }
    
    public string? PasswordResetCode { get; set; }
    
    public DateTime? PasswordResetCodeExpiration { get; set; }
    
    public CoachDetail CoachDetail { get; set; }
    
    public StudentDetail StudentDetail { get; set; }
    public ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();
    
    public ICollection<Invitation> InvitationsAsCoach { get; set; } = new List<Invitation>();
    
    public ICollection<Invitation> InvitationsAsStudent { get; set; } = new List<Invitation>();

    public ICollection<CoachYksRanking> CoachYksRankings { get; set; } = new List<CoachYksRanking>();

    public PrivateTutoringTYT PrivateTutoringTYT { get; set; }
    public PrivateTutoringMF PrivateTutoringMF { get; set; }
    public PrivateTutoringTM PrivateTutoringTM { get; set; }
    public PrivateTutoringSozel PrivateTutoringSozel { get; set; }
    public PrivateTutoringDil PrivateTutoringDil { get; set; }
    
    public TYTNets TytNets { get; set; }
    public TMNets TmNets { get; set; }
    public MFNets MfNets { get; set; }
    public SozelNets SozelNets { get; set; }
    public DilNets DilNets { get; set; }
    public ICollection<CoachStudentTrainingSchedule> CoachStudentTrainingSchedules { get; set; } = new List<CoachStudentTrainingSchedule>();

    public ICollection<CoachStudentTrainingSchedule> StudentCoachTrainingSchedules { get; set; } = new List<CoachStudentTrainingSchedule>();
}

public class ApplicationRole : IdentityRole<Guid>
{
    
}

public class SubscriptionHistory
{
    public int Id { get; set; } // Abonelik geçmişi kimliği

    public Guid UserId { get; set; } // Kullanıcı kimliği
    
    // ForeignKey ile ilişkiyi kurmak için ApplicationUser sınıfına referans
    public ApplicationUser User { get; set; }

    public DateTime SubscriptionStartDate { get; set; } // Abonelik başlangıç tarihi

    public DateTime? SubscriptionEndDate { get; set; } // Abonelik bitiş tarihi

    public SubscriptionType Type { get; set; } // Abonelik tipi (örneğin, ücretsiz, standart, premium)

    public string SubscriptionId { get; set; }
    
    public string InvoiceId { get; set; }
    
    public string SubscriptionState { get; set; }
    
    public string InvoiceState { get; set; }
    
    public string LastError { get; set; }
}

public enum SubscriptionType
{
    Pdr = 0,
    Coach = 1,
}

public class StudentDetail
{
    public Guid StudentId { get; set; }

    public string? Name { get; set; }
    
    public string? Surname { get; set; }
    
    public string? Email { get; set; }
    
    public string? Mobile { get; set; }
    
    public string? ParentName { get; set; }
    
    public string? ParentSurname { get; set; }
    
    public string? ParentEmail { get; set; }
    
    public string? ParentMobile { get; set; }
    
    public string? HighSchool { get; set; }
    
    public float? HighSchoolGPA { get; set; }
    
    public byte? TytGoalNet { get; set; }
    
    public byte? AytGoalNet { get; set; }
    
    public uint? GoalRanking { get; set; }

    public string? DesiredProfessionSchoolField { get; set; }
    
    public string? ExpectationsFromCoaching { get; set; }

    public bool? School { get; set; }
    
    public bool? Course { get; set; }
    
    public bool? Youtube { get; set; }
    
    public bool? PrivateTutoringTyt { get; set; }
    
    public bool? PrivateTutoringAyt { get; set; }
    
    public Grade Grade { get; set; }
    
    public ExamType ExamType { get; set; }
    
    public ApplicationUser Student { get; set; } // İlişkiyi burada tanımlıyoruz
    
    public StudentStatus Status { get; set; }
}

public enum DepartmentType
{
    TM,
    MF,
    Sozel,
    Dil,
}

public class PrivateTutoringTYT
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
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

public class PrivateTutoringMF
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    public bool Mathematics { get; set; }
    public bool Geometry { get; set; }
    public bool Physics { get; set; }
    public bool Chemistry { get; set; }
    public bool Biology { get; set; }
}
    
public class PrivateTutoringTM
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
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
public class PrivateTutoringSozel
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
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
    
public class PrivateTutoringDil
{
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //YDT: (Max 80, Min 0)
    public bool YTD { get; set; }
}

public class CoachYksRanking
{
    public Guid Id { get; set; }
    
    public Guid CoachId { get; set; }
    
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz

    public string Year { get; set; }
    
    public bool Enter { get; set; }
    
    public uint Rank { get; set; }
}

public class CoachDetail
{
    public Guid CoachId { get; set; }
    
    public ApplicationUser Coach { get; set; } // İlişkiyi burada tanımlıyoruz

    public string Name { get; set; }
    public string Surname { get; set; }
    public string Mobile { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }
        
    public DepartmentType DepartmentType { get; set; }
    
    public Guid UniversityId { get; set; }
    
    // Universite referansı
    public University University { get; set; } 
    
    public Guid DepartmentId { get; set; }
    
    // Department referansı
    public Department Department { get; set; }
    
    public string HighSchool { get; set; }

    public float HighSchoolGPA { get; set; }
    public byte FirstTytNet { get; set; }
        
    public byte LastTytNet { get; set; }
        
    public byte FirstAytNet { get; set; }
        
    public byte LastAytNet { get; set; }
    
    public bool ChangedDepartmentType { get; set; }

    public DepartmentType FromDepartment { get; set; }
        
    public DepartmentType ToDepartment { get; set; }

    // public bool Tm { get; set; }
    // public bool Mf { get; set; }
    // public bool Sozel { get; set; }
    // public bool Dil { get; set; }
    // public bool Tyt { get; set; }

    public bool IsGraduated { get; set; }
    
    public bool UsedYoutube { get; set; }
    
    public bool GoneCramSchool { get; set; }
    
    public bool School { get; set; }
    
    public bool PrivateTutoring { get; set; }
    
    public bool Male { get; set; }
    public uint Rank { get; set; }
    public byte StudentQuota { get; set; }
    public byte MondayQuota { get; set; }
    
    public byte TuesdayQuota { get; set; }
    
    public byte WednesdayQuota { get; set; }
    
    public byte ThursdayQuota { get; set; }
    
    public byte FridayQuota { get; set; }
    
    public byte SaturdayQuota { get; set; }
    
    public byte SundayQuota { get; set; }
}

public class TYTNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Anlam Bilgisi: (Max 30, Min 0)
    public byte? Semantics { get; set; }
    //Dil Bilgisi: (Max 10, Min 0)
    public byte? Grammar { get; set; }
    //Matematik: (Max 30, Min 0)
    public byte? Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte? Geometry { get; set; }
    //Tarih: (Max 5, Min 0)
    public byte? History { get; set; }
    //Coğrafya: (Max 5, Min 0)
    public byte? Geography { get; set; }
    //Felsefe: (Max 5, Min 0)
    public byte? Philosophy { get; set; }
    //Din: (Max 5, Min 0)
    public byte? Religion { get; set; }
    //Fizik: (Max 7, Min 0)
    public byte? Physics { get; set; }
    //Kimya: (Max 7, Min 0)
    public byte? Chemistry { get; set; }
    //Biology: (Max 6, Min 0)
    public byte? Biology { get; set; }
}

public class MFNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Matematik: (Max 30, Min 0)
    public byte? Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte? Geometry { get; set; }
    //Fizik: (Max 14, Min 0)
    public byte? Physics { get; set; }
    //Kimya: (Max 13, Min 0)
    public byte? Chemistry { get; set; }
    //Biology: (Max 13, Min 0)
    public byte? Biology { get; set; }
}
    
public class TMNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Matematik: (Max 30, Min 0)
    public byte? Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte? Geometry { get; set; }
    //Edebiyat: (Max 24, Min 0)
    public byte? Literature { get; set; }
    //Tarih: (Max 10, Min 0)
    public byte? History { get; set; }
    //Coğrafya: (Max 6, Min 0)
    public byte? Geography { get; set; }
}
    
//Sözel
public class SozelNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //Tarih-1: (Max 10, Min 0)
    public byte? History1 { get; set; }
    //Coğrafya: (Max 24, Min 0)
    public byte? Geography1 { get; set; }
    //Edebiyat-1: (Max 6, Min 0)
    public byte? Literature1 { get; set; }
    //Tarih-2: (Max 11, Min 0)
    public byte? History2 { get; set; }
    //Coğrafya-2: (Max 11, Min 0)
    public byte? Geography2 { get; set; }
    //Felsefe: (Max 12, Min 0)
    public byte? Philosophy { get; set; }
    //Din: (Max 6, Min 0)
    public byte? Religion { get; set; }
}
    
public class DilNets
{
    public Guid UserId { get; set; }
    
    public ApplicationUser User { get; set; } // İlişkiyi burada tanımlıyoruz
    
    //YDT: (Max 80, Min 0) Yabacnı Dil Testi
    public byte? YDT { get; set; }
}

public class University
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public ICollection<UniversityDepartment> UniversityDepartments { get; set; } = new List<UniversityDepartment>();
}

public class Department
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public DepartmentType DepartmentType { get; set; }
    
    public ICollection<UniversityDepartment> UniversityDepartments { get; set; } = new List<UniversityDepartment>();
}

public enum ExamType
{
    TYT,
    TM,
    MF,
    Sozel,
    Dil,
}

public enum Grade
{
    Dokuz,
    On,
    Onbir,
    Oniki,
    Mezun
}

public class UniversityDepartment
{
    public Guid UniversityId { get; set; }
    public University University { get; set; }

    public Guid DepartmentId { get; set; }
    public Department Department { get; set; }
}

public class Invitation
{
    public Guid Id { get; set; }
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; }
    public Guid StudentId { get; set; }
    public ApplicationUser Student { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }

    public InvitationType Type { get; set; }
    
    public InvitationState State { get; set; }
    
    public bool IsAvailable { get; set; } // true ise görüşme aktif, false ise zamanı gelmedi // 5 dakika kala burası açılır

    public Guid? ZoomMeetDetailId { get; set; }
    
    public ZoomMeetDetail ZoomMeetDetail { get; set; }
}

public class CoachStudentTrainingSchedule
{
    public Guid Id { get; set; }
    public Guid CoachId { get; set; }
    public ApplicationUser Coach { get; set; }
    public Guid StudentId { get; set; }
    public ApplicationUser Student { get; set; }
    public DayOfWeek Day { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class ZoomMeetDetail
{
    public Guid Id { get; set; }
    
    public string? MeetId { get; set; }
    
    public string? HostEmail { get; set; }
    
    public string? RegistrationUrl { get; set; }
    
    public string? JoinUrl { get; set; }
    
    public string? MeetingPasscode { get; set; }
    
    public DateTime? StartTime { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public string? CoachRegistrantId { get; set; }
    
    public string? CoachParticipantPinCode { get; set; }
    
    public string? CoachJoinUrl { get; set; }
    
    public string? StudentRegistrantId { get; set; }
    
    public string? StudentParticipantPinCode { get; set; }
    
    public string? StudentJoinUrl { get; set; }
    
    public Guid InvitationId { get; set; }
    public Invitation Invitation { get; set; }
}

public enum InvitationType
{
    VideoCall,
    VoiceCall,
}

public enum InvitationState
{
    SpecifyHour, // Saat Belirle(Sadece Koç belirleyebiliyor)
    WaitingApprove,
    Approved, // Görüşülecek
    Cancelled, //İptal edildi
    Open, // Açık
    Done, //Tamamlandı
}

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

public enum UserType
{
    Student,
    Coach,
    Pdr,
}



