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

// 1️⃣ Carrega o .env só em desenvolvimento local
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load(); // carrega .env da raiz automaticamente
}

var builder = WebApplication.CreateBuilder(args);

// 2️⃣ JWT
var jwtSecretKey = Environment.GetEnvironmentVariable("Jwt__Key")
                   ?? "chave_mestra_padrao_para_evitar_erros_de_nulo_123";
var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer");
var audience = Environment.GetEnvironmentVariable("Jwt__Audience");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// 3️⃣ Connection string PostgreSQL
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? throw new Exception("Variável de ambiente ConnectionStrings__DefaultConnection não encontrada!");

// 4️⃣ Npgsql + enums
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<Status>("status_enum");
dataSourceBuilder.MapEnum<Prioridade>("prioridade_enum");
var dataSource = dataSourceBuilder.Build();

// 5️⃣ DbContext
builder.Services.AddDbContext<SistemaDeTarefaDBContext>(options =>
{
    options.UseNpgsql(dataSource);
});

// 6️⃣ Injeção de dependências
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// 7️⃣ Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 8️⃣ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 9️⃣ CORS seguro
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("https://task-management-client-react.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// 10️⃣ Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GerenciadorDeTarefas API V1");
        c.RoutePrefix = string.Empty; // Swagger abre na raiz
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 11️⃣ Migração automática (cuidado em múltiplas instâncias)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SistemaDeTarefaDBContext>();
    db.Database.Migrate();
}

app.Run();
