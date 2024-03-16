using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NightOwlEnterprise.Api;
using NightOwlEnterprise.Api.Endpoints.Students;
using NightOwlEnterprise.Api.Utils.Email;
using NLog;
using NLog.Web;
using Stripe;

var logger = LogManager.Setup()
                       .LoadConfigurationFromAppSettings()
                       .GetCurrentClassLogger();

logger.Fatal("Logger is created.");

var builder = WebApplication.CreateBuilder(args);

logger.Fatal("Builder is created.");

builder.Host.UseNLog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        logger.Fatal("ApplicationDbContext is using memory database.");
        options.UseInMemoryDatabase("AppDb");    
    }
    else
    {
        logger.Fatal("ApplicationDbContext is using postgresql.");
        options.UseNpgsql(connectionString);
    }
});

builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<TurkishIdentityErrorDescriber>();

builder.Services.AddSingleton<StudentEmailSender>();

var smtpServerCredentialEnabled = builder.Configuration.GetValue<bool>("SmtpServer:Enabled");

if (smtpServerCredentialEnabled)
{
    builder.Services.Configure<SmtpServerCredential>(
        builder.Configuration.GetSection(SmtpServerCredential.SmtpServer));
    builder.Services.AddTransient(typeof(IEmailSender<ApplicationUser>), typeof(StudentIdentityEmailSender));
    builder.Services.AddTransient<IEmailSender, ProductionEmailSender>();    
}
else
{
    // builder.Services.TryAddTransient(typeof(IEmailSender<>), typeof(DefaultMessageEmailSender<>));
    // builder.Services.TryAddTransient<IEmailSender, NoOpEmailSender>();
    builder.Services.AddTransient<IEmailSender, LocalEmailSender>();    
}

builder.Services.Configure<StripeCredential>(
    builder.Configuration.GetSection(StripeCredential.Stripe));

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
});

builder.Services.AddProblemDetails();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.UseExceptionHandler();

//app.UseStatusCodePages();

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

app.MapControllers();

logger.Fatal("App is created.");

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

app.MapStudentsIdentityApi<ApplicationUser>();

var stripeCredential = app.Services.GetService<IOptions<StripeCredential>>()?.Value;
// This is a public sample test API key.
// Don’t submit any personally identifiable information in requests made with this key.
// Sign in to see your own test API key embedded in code samples.
// Stripe.StripeConfiguration.ApiKey = "sk_test_4eC39HqLyjWDarjtT1zdp7dc";
// Stripe.StripeConfiguration.ApiKey = "sk_test_51OsO7cEyxtA03PfNAGZBvY40v3lzbZLF7Bb0BYOG8wRdlXnLhJoCXUIjtIOCyZtawn5lh97dnu6O0J5jcMxDL00O00WekY3Ta7";
if (string.IsNullOrEmpty(stripeCredential.SecretKey))
{
    logger.Fatal("Stripe is not active.");    
}
else
{
    logger.Fatal("Stripe is active.");
    Stripe.StripeConfiguration.ApiKey = stripeCredential.SecretKey;    
}

app.Run();

logger.Fatal("App is running.");

public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }
}

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser<Guid>
{
    public string Name { get; set; } = String.Empty;
    public string Address { get; set; }  = String.Empty;
    public string City { get; set; }  = String.Empty;
    
    public AccountStatus AccountStatus { get; set; }
    
    public UserType UserType { get; set; }
}

public class ApplicationRole : IdentityRole<Guid>
{
    
}

public enum AccountStatus
{
    Active,
    PaymentAwaiting,
}

public enum UserType
{
    Student,
    Coach,
}


