using GerenciadorDeTarefas.Data;
using GerenciadorDeTarefas.Models;
using GerenciadorDeTarefas.Repository;
using GerenciadorDeTarefas.Repository.Interface;
using GerenciadorDeTarefas.Service;
using GerenciadorDeTarefas.Service.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// =====================
// ENV (.env apenas local)
// =====================
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// =====================
// JWT
// =====================
var jwtSecretKey = Environment.GetEnvironmentVariable("Jwt__Key")
    ?? throw new Exception("Jwt__Key não configurado");

var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? "default_issuer";
var audience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? "default_audience";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// =====================
// CONNECTION STRING
// =====================
// Pode simplificar a conexão novamente
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SistemaDeTarefaDBContext>(options =>
    options.UseNpgsql(connectionString));


// =====================
// DEPENDENCY INJECTION
// =====================
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// =====================
// CONTROLLERS + JSON
// =====================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// =====================
// SWAGGER
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================
// CORS
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel", policy =>
    {
        policy.WithOrigins("https://task-management-client-react.vercel.app", "http://localhost:5173/")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// =====================
// SWAGGER (DEV ONLY)
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GerenciadorDeTarefas API V1");
        c.RoutePrefix = string.Empty;
    });
}

// =====================
// PIPELINE
// =====================
app.UseRouting();
app.UseCors("AllowVercel");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// =====================
// AUTO MIGRATIONS
// =====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider
            .GetRequiredService<SistemaDeTarefaDBContext>();

        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations: {ex}");
    }
}


app.Run();
