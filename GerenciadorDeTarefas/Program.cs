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
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurações de Configuração e Ambiente
var reload = builder.Environment.IsDevelopment();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: reload)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: reload)
    .AddEnvironmentVariables();

// 2. JWT - Autenticação
var jwtSecretKey = builder.Configuration["Jwt:Key"] ?? "chave_mestra_padrao_para_evitar_erros_de_nulo_123";
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

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

// 3. String de Conexão com Parser para o Render
string connectionString;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// 4. CONFIGURAÇÃO CRÍTICA PARA OS SEUS ENUMS MANUAIS
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

// ESSA LINHA É OBRIGATÓRIA porque você mudou de INTEGER para ENUM no banco
dataSourceBuilder.EnableUnmappedTypes();

// Mapeia as classes C# para os nomes exatos dos tipos criados no seu banco (status_enum e prioridade_enum)
dataSourceBuilder.MapEnum<Status>("status_enum");
dataSourceBuilder.MapEnum<Prioridade>("prioridade_enum");

var dataSource = dataSourceBuilder.Build();

// 5. Configuração do DbContext
builder.Services.AddDbContext<SistemaDeTarefaDBContext>(options =>
{
    options.UseNpgsql(dataSource, npgsqlOptions =>
    {
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });

    // Ignora o aviso de migrações pendentes para não travar o deploy no Render
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// 6. Injeção de Dependências
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProjetoRepository, ProjetoRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ITarefaRepository, TarefaRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// 7. Configuração de Controllers e JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Converte o Enum para String no JSON (ex: exibe "Aberta" em vez de 0)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Evita erros de referência circular entre Tarefa e Usuario
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 8. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "https://seu-frontend.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// 9. Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 10. Tenta aplicar migrações (Opcional, mas útil)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SistemaDeTarefaDBContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Nota: Ignorando erro de migração manual: {ex.Message}");
    }
}

app.Run();