using NLog;
using NLog.Web;

var logger = LogManager.Setup()
                       .LoadConfigurationFromAppSettings()
                       .GetCurrentClassLogger();

logger.Fatal("Logger is created.");

var builder = WebApplication.CreateBuilder(args);

logger.Fatal("Builder is created.");

builder.Host.UseNLog();

var app = builder.Build();

logger.Fatal("App is created.");

app.MapGet("/", (ILogger<Program> logger) =>
{
    return "Hello World!";
});

app.MapGet("/by", () => "Servet ÞEKER");

app.Run();

logger.Fatal("App is running.");
