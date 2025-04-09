using AtivoPlus.Data;
using AtivoPlus.Logic;
using AtivoPlus.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;
using System.Reflection;




var builder = WebApplication.CreateBuilder(args);

// Adicionar o Entity Framework com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));

builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Adiciona suporte para Web API


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});


var app = builder.Build();

Console.WriteLine($"Environment: {app.Environment.EnvironmentName}"); // Para depuração
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//test 

ExtraLogic.SetUpAdminPermission(AppDbContext.GetDb());
if (args.Length > 0 && args[0] == "--addadmin")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Erro: Necessário fornecer um nome de utilizador.");
        return;
    }

    var username = args[1];
    var db = AppDbContext.GetDb();

    int? userID = await UserLogic.GetUserID(db, username);
    if (userID == null)
    {
        Console.WriteLine("User does not exist");
        return;
    }

    int? permID = await PermissionLogic.GetPermissionID(db, "admin");
    if (permID == null)
    {
        Console.WriteLine("Permission does not exist");
        return;
    }

    if (await db.AddUserPermission(userID.Value, permID.Value) == false)
    {
        Console.WriteLine("Could not add permission");
        return;
    }

    Console.WriteLine($"Administrador '{username}' adicionado com sucesso!");
    return;
}
// Inicializa com a tua chave (podes ler do .env ou appsettings.json)
Env.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env"));
var apiKey = Environment.GetEnvironmentVariable("TWELVE_API")!;
if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: environment variable 'TWELVE_API' not found. Exiting.");
    Environment.Exit(1);
}
TwelveDataLogic.StartTwelveDataLogic(apiKey);


app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers(); // Habilita Web API

ExtraLogic.CheckUsersTokens();
app.Run();

