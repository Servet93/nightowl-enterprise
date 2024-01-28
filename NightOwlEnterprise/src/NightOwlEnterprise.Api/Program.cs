using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NightOwlEnterprise.Api.Endpoints.Students;
using NightOwlEnterprise.Api.Utils.Email;
using NLog;
using NLog.Web;

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

builder.Services.AddIdentityApiEndpoints<StudentApplicationUser>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton<TurkishIdentityErrorDescriber>();

var smtpServerCredentialEnabled = builder.Configuration.GetValue<bool>("SmtpServerCredential:Enabled");

if (smtpServerCredentialEnabled)
{
    builder.Services.Configure<SmtpServerCredential>(
        builder.Configuration.GetSection(SmtpServerCredential.SmtpServer));
    builder.Services.AddTransient(typeof(IEmailSender<StudentApplicationUser>), typeof(StudentIdentityEmailSender));
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
    
    options.Password.RequireDigit = true;       // En az bir rakam içermeli
    options.Password.RequireLowercase = true;   // En az bir küçük harf içermeli
    options.Password.RequireUppercase = true;   // En az bir büyük harf içermeli
    //options.Password.RequireNonAlphanumeric = true; // En az bir özel karakter içermeli
    options.Password.RequiredLength = 8;        // Minimum şifre uzunluğu
    options.Password.RequiredUniqueChars = 4;    // Şifrede kaç farklı karakter olmalı
    
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

var app = builder.Build();

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

app.MapStudentsIdentityApi<StudentApplicationUser>();

app.Run();

logger.Fatal("App is running.");




public record User(int Id);

public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<StudentApplicationUser>
{
    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }
}

public class StudentApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
{
    public string Name { get; set; } = String.Empty;
    
    public string Surname { get; set; }  = String.Empty;
}


