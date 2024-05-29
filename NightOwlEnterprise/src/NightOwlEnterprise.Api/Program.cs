using System.Globalization;
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
using NightOwlEnterprise.Api;
using NightOwlEnterprise.Api.Endpoints;
using NightOwlEnterprise.Api.Endpoints.Coachs;
using NightOwlEnterprise.Api.Endpoints.Students;
using NightOwlEnterprise.Api.Endpoints.Universities;
using NightOwlEnterprise.Api.Entities;
using NightOwlEnterprise.Api.Entities.Enums;
using NightOwlEnterprise.Api.Entities.Nets;
using NightOwlEnterprise.Api.Entities.PrivateTutoring;
using NightOwlEnterprise.Api.Utils.Email;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;
using NLog.Layouts;
using NLog.Web;
using Swashbuckle.AspNetCore.Filters;
using JsonAttribute = NLog.Layouts.JsonAttribute;

var builder = WebApplication.CreateBuilder(args);

var systemStartPointNow = DateTime.Now;
var systemStartPointUtcNow = DateTime.UtcNow;

var systemEndPointNow = DateTime.Now;
var systemEndPointUtcNow = DateTime.UtcNow;

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
    
    // swaggerGenOptions.OperationFilter<FileUploadOperationFilter>();
    
    swaggerGenOptions.MapType<IFormFile>(() => new OpenApiSchema
    {
        // Type = "string", Format = "binary"
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["file"] = new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            }
        },
        Required = new HashSet<string> { "file" }
    }); // IFormFile için özel belgeleme
    
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

// builder.Services.AddHostedService<SeedDataService>();

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
app.MapUniversityApi();

TimeZoneInfo localZone = TimeZoneInfo.Local;
CultureInfo currentCulture = CultureInfo.CurrentCulture;
var turkeyTimeZone = DateTimeExtensions.TimeZone;

app.MapGet("/conf", async context =>
{
    // ILoggerFactory örneği al
    var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
    
    // ILogger örneğini oluştur
    var logger = loggerFactory.CreateLogger("ConfigEndpoint");
    
    logger.LogInformation("Aws Cloud Watch Conf");
    
    StringBuilder sb = new();
    sb.AppendLine($"TurkeyTimeZone.DisplayName -> {turkeyTimeZone.DisplayName}");
    sb.AppendLine($"TurkeyTimeZone.BaseUtcOffset -> {turkeyTimeZone.BaseUtcOffset}");
    sb.AppendLine($"TurkeyTimeZone.SystemStartPointNow -> {systemStartPointNow.ConvertToTimeZone()}");
    sb.AppendLine($"TurkeyTimeZone.SystemStartPointUtcNow -> {systemStartPointUtcNow.ConvertUtcToTimeZone()}");
    sb.AppendLine($"TurkeyTimeZone.SystemEndPointNow -> {systemEndPointNow.ConvertToTimeZone()}");
    sb.AppendLine($"TurkeyTimeZone.SystemEndPointUtcNow -> {systemEndPointUtcNow.ConvertUtcToTimeZone()}");
    sb.AppendLine($"---------------------------------------------------------------");
    sb.AppendLine($"LocalZone.DisplayName -> {localZone.DisplayName}");
    sb.AppendLine($"LocalZone.BaseUtcOffset -> {localZone.BaseUtcOffset}");
    sb.AppendLine($"LocalZone.SystemStartPointNow -> {systemStartPointNow}");
    sb.AppendLine($"LocalZone.SystemStartPointUtcNow -> {systemStartPointUtcNow}");
    sb.AppendLine($"LocalZone.SystemEndPointNow -> {systemEndPointNow}");
    sb.AppendLine($"LocalZone.SystemEndPointUtcNow -> {systemEndPointUtcNow}");
    sb.AppendLine($"---------------------------------------------------------------");
    sb.AppendLine($"TurkeyTimeZone.SystemNow -> {DateTime.Now.ConvertToTimeZone()}");
    sb.AppendLine($"TurkeyTimeZone.SystemUtcNow -> {DateTime.UtcNow.ConvertUtcToTimeZone()}");
    sb.AppendLine($"LocalZone.SystemNow -> {DateTime.Now}");
    sb.AppendLine($"LocanZone.SystemUtcNow -> {DateTime.UtcNow}");
    sb.AppendLine($"---------------------------------------------------------------");
    sb.AppendLine($"Culture.DisplayName -> {currentCulture.DisplayName}");
    sb.AppendLine($"---------------------------------------------------------------");
    sb.AppendLine($"IsMongoEnabled -> {isMongoEnabled}");
    sb.AppendLine($"Mongo -> {mongoConnectionString}");
    sb.AppendLine($"IsPostgresEnabled -> {isPostgresEnabled}");
    sb.AppendLine($"Postgres -> {postgresConnectionString}");
    sb.AppendLine($"---------------------------------------------------------------");
    sb.AppendLine($"Stripe.PublishableKey -> {stripeCredential.PublishableKey}");
    sb.AppendLine($"Stripe.SecretKey -> {stripeCredential.SecretKey}");
    sb.AppendLine($"Stripe.SigningSecret -> {stripeCredential.SigningSecret}");
    sb.AppendLine($"Stripe.DereceliKocPriceId -> {stripeCredential.DereceliKocPriceId}");
    sb.AppendLine($"Stripe.PdrPriceId -> {stripeCredential.PdrPriceId}");
    sb.AppendLine($"---------------------------------------------------------------");
    sb.AppendLine($"JwtConfig.Audience -> {jwtConfig!.Audience}");
    sb.AppendLine($"JwtConfig.Issuer -> {jwtConfig.Issuer}");
    sb.AppendLine($"JwtConfig.Key -> {jwtConfig.Key}");
    sb.AppendLine($"---------------------------------------------------------------");
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

systemEndPointNow = DateTime.Now;
systemEndPointUtcNow = DateTime.UtcNow;

app.Run();

logger.Fatal("App is running.");

async Task SeedCoachs(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
{
    var hasAnyCoach = dbContext.Users.Any(x => x.UserType == UserType.Coach);
    
    var rnd = new Random();

    var index = 0;
    
    if (!hasAnyCoach)
    {
        var universities = dbContext.Universities.ToList();
        var universitiesCount = universities.Count;
        
        // universities * departments(39) * 5 = 195
        var persons = PersonGenerator.GeneratePeople();
        
        foreach (var person in persons)
        {
            index++;
            
            var x = (byte)rnd.Next(5);

            DepartmentType departmentType = DepartmentType.Sozel;

            var remain = index % 4;
            
            if (remain == 1)
            {
                departmentType = DepartmentType.TM;
            }
            else if (remain == 2)
            {
                departmentType = DepartmentType.MF;
            }
            else if (remain == 3)
            {
                departmentType = DepartmentType.Sozel;
            }
            else if (remain == 4)
            {
                departmentType = DepartmentType.Dil;
            }
            
            var mondayQuota = x == 0 ? (byte)1 : (byte)rnd.Next(5);
            var tuesdayQuota = x == 1 ? (byte)1 : (byte)rnd.Next(5);
            var wednesdayQuota = x == 2 ? (byte)1 : (byte)rnd.Next(5);
            var thursayQuota = x == 3 ? (byte)1 : (byte)rnd.Next(5);
            var fridayQuota = x == 4 ? (byte)1 : (byte)rnd.Next(5);

            var studentQuota = (byte)(mondayQuota + tuesdayQuota + wednesdayQuota + thursayQuota + fridayQuota);
            
            var applicationUser = new ApplicationUser()
                {
                    Id = person.Id,
                    Name = person.FirstName.ReplaceTurkishCharacters() +" "+ person.LastName.ReplaceTurkishCharacters(),
                    UserName = person.FirstName.ReplaceTurkishCharacters() + person.LastName.ReplaceTurkishCharacters(),
                    Email = person.Email,
                    PhoneNumber = person.Phone,
                    City = person.City,
                    Address = person.Address,
                    CoachDetail = new CoachDetail()
                    {
                        Name = person.FirstName,
                        Surname = person.LastName,
                        Email = person.Email,
                        Male = person.Gender,
                        BirthDate = DateTime.Now,
                        DepartmentType = departmentType,
                        DepartmentName = $"Department Name -> {rnd.Next(universitiesCount)}",
                        Mobile = person.Phone,
                        UniversityId = universities[rnd.Next(universitiesCount)].Id,
                        FirstTytNet = (byte)rnd.Next(90, 120),
                        LastTytNet = (byte)rnd.Next(90, 120),
                        FirstAytNet = (byte)rnd.Next(60, 80),
                        LastAytNet = (byte)rnd.Next(60, 80),
                        School = rnd.Next(100) % 2 == 0,
                        UsedYoutube = rnd.Next(100) % 2 == 0,
                        GoneCramSchool = rnd.Next(100) % 2 == 0,
                        Rank = (uint)rnd.Next(5000),
                        IsGraduated = rnd.Next(6) != 1 ? true : false,
                        HighSchool = person.HighSchoolName,
                        HighSchoolGPA = person.HighSchoolScore,
                        StudentQuota = studentQuota,
                        MondayQuota = mondayQuota,
                        TuesdayQuota = tuesdayQuota,
                        WednesdayQuota = wednesdayQuota,
                        ThursdayQuota = thursayQuota,
                        FridayQuota = fridayQuota,
                        PrivateTutoring = true,
                        Status = CoachStatus.Active,
                    },
                    TytNets = new TYTNets()
                    {
                        Biology = (byte)rnd.Next(6),
                        Chemistry = (byte)rnd.Next(7),
                        Geometry = (byte)rnd.Next(10),
                        Physics = (byte)rnd.Next(7),
                        Mathematics = (byte)rnd.Next(30),
                        Geography = (byte)rnd.Next(5),
                        History = (byte)rnd.Next(5),
                        Philosophy = (byte)rnd.Next(5),
                        Religion = (byte)rnd.Next(5),
                        Turkish = (byte)rnd.Next(40),
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
                
                if (departmentType == DepartmentType.MF)
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
                else if (departmentType == DepartmentType.TM)
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
                else if (departmentType == DepartmentType.Sozel)
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
                else if (departmentType == DepartmentType.Dil)
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
                    await userManager.CreateAsync(applicationUser, "Aa123456").ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
        }
    }
}

public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<University> Universities { get; set; }
    public DbSet<CoachDetail> CoachDetail { get; set; }
    public DbSet<StudentDetail> StudentDetail { get; set; }
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
    
    public DbSet<ResourcesTYT> ResourcesTYT { get; set; }
    public DbSet<ResourcesAYT> ResourcesAYT { get; set; }
    
    public DbSet<ProfilePhoto> ProfilePhotos { get; set; }

    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options) :
        base(options)
    {
        
    }
    
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
        
        builder.Entity<CoachStudentTrainingSchedule>()
            .Property(cc => cc.CreatedAt)
            .HasColumnType("timestamp without time zone");
        
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
        
        // ResourcesTYT tablosunun ilişkilerini belirtiyoruz
        builder.Entity<ResourcesTYT>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // ResourcesTYT ile ApplicationUser arasında ilişki
        builder.Entity<ResourcesTYT>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.ResourcesTYT)
            .HasForeignKey<ResourcesTYT>(cd => cd.UserId);
        
        // ResourcesTYT tablosunun ilişkilerini belirtiyoruz
        builder.Entity<ResourcesAYT>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
        
        // ResourcesTYT ile ApplicationUser arasında ilişki
        builder.Entity<ResourcesAYT>()
            .HasOne(cd => cd.User)
            .WithOne(u => u.ResourcesAYT)
            .HasForeignKey<ResourcesAYT>(cd => cd.UserId);
        
        // ProfilePhoto tablosunun ilişkilerini belirtiyoruz
        builder.Entity<ProfilePhoto>()
            .HasKey(cd => cd.UserId); // Primary key'i belirtiyoruz
    }
}