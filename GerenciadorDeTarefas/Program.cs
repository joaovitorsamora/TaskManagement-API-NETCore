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

// Carrega o .env apenas em desenvolvimento local
if (builder.Environment.IsDevelopment())
{
    DotNetEnv.Env.Load();
}

// Configurações de JWT
var jwtSecretKey = Environment.GetEnvironmentVariable("Jwt__Key")
                    ?? "chave_mestra_padrao_para_evitar_erros_de_nulo_123";
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
        };
    });

// 1. EXTRAÇÃO E VALIDAÇÃO DA CONNECTION STRING
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new Exception("String de conexão 'DefaultConnection' não encontrada!");

// 2. MAPEAMENTO DE ENUMS PARA O POSTGRESQL (Npgsql)
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<Status>("status_enum");
dataSourceBuilder.MapEnum<Prioridade>("prioridade_enum");
var dataSource = dataSourceBuilder.Build();

// 3. CONFIGURAÇÃO DO DBCONTEXT USANDO O DATASOURCE MAPEADO
builder.Services.AddDbContext<SistemaDeTarefaDBContext>(options =>
    options.UseNpgsql(dataSource,
    npgsqlOptions => npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Injeção de Dependência
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CONFIGURAÇÃO DE CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVercel",
        policy =>
        {
            policy.WithOrigins("https://task-management-client-react.vercel.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// --- ALTERAÇÃO AQUI: SWAGGER ESCONDIDO EM PRODUÇÃO ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GerenciadorDeTarefas API V1");
        c.RoutePrefix = string.Empty; // No local, abre direto no Swagger
    });
}

// ORDEM DOS MIDDLEWARES
app.UseRouting();

app.UseCors("AllowVercel");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// APLICAÇÃO DE MIGRATIONS AUTOMÁTICAS
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDeTarefaDBContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
    }
}

app.Run();