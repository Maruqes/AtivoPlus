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

//create admin permission if it does not exist
void setUpAdminPermission()
{
    var db = getDb();

    // Verifica se a permissão 'admin' já existe
    if (!db.Permissions.Any(p => p.Id == -1))
    {
        var adminPermission = new Permission
        {
            Id = -1,
            Name = "admin"
        };

        db.Permissions.Add(adminPermission);
        db.SaveChanges();
    }
}

var builder = WebApplication.CreateBuilder(args);

// Adicionar o Entity Framework com PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));

builder.Services.AddRazorPages();
builder.Services.AddControllers(); // Adiciona suporte para Web API

var app = builder.Build();

setUpAdminPermission();
if (args.Length > 0 && args[0] == "--addadmin")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Erro: Necessário fornecer um nome de utilizador.");
        return;
    }

    var username = args[1];
    var db = getDb();

    int userID = await UserLogic.GetUserID(db, username);
    if (userID == -1)
    {
        Console.WriteLine("User does not exist");
        return;
    }

    int permID = await PermissionLogic.GetPermissionID(db, "admin");
    if (permID == -2)
    {
        Console.WriteLine("Permission does not exist");
        return;
    }

    if (await db.AddUserPermission(userID, permID) == false)
    {
        Console.WriteLine("Could not add permission");
        return;
    }

    Console.WriteLine($"Administrador '{username}' adicionado com sucesso!");
    return;
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers(); // Habilita Web API

ExtraLogic.CheckUsersTokens();
app.Run();

