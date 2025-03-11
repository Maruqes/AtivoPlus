using AtivoPlus.Data;
using AtivoPlus.Logic;
using AtivoPlus.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


AppDbContext getDb()
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

    string connectionString = configuration.GetConnectionString("PostgreSqlConnection") ?? throw new InvalidOperationException("Connection string 'PostgreSqlConnection' not found.");

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(connectionString)
        .Options;

    var db = new AppDbContext(options);
    db.Database.EnsureCreated();
    return db;
}



var builder = WebApplication.CreateBuilder(args);

// Adicionar o Entity Framework com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));

builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Adiciona suporte para Web API


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

Console.WriteLine($"Environment: {app.Environment.EnvironmentName}"); // Para depuração
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


ExtraLogic.SetUpAdminPermission(getDb());
if (args.Length > 0 && args[0] == "--addadmin")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Erro: Necessário fornecer um nome de utilizador.");
        return;
    }

    var username = args[1];
    var db = getDb();

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
AlphaVantageLogic.StartAlphaVantageLogic();
Console.WriteLine("AlphaVantageLogic started");


app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers(); // Habilita Web API

ExtraLogic.CheckUsersTokens();
app.Run();

