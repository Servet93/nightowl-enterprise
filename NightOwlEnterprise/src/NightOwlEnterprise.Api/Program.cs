var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var isWorkingOnEc2 = configuration.GetValue<bool>("WorkingOnEc2");

Console.WriteLine($"WorkingOnEc2 -> {isWorkingOnEc2}");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/by", () => "Servet ÞEKER");

app.Run();
