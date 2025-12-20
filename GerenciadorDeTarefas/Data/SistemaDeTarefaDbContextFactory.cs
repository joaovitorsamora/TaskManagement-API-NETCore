using GerenciadorDeTarefas.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace GerenciadorDeTarefas.Data
{
    public class SistemaDeTarefaDbContextFactory
        : IDesignTimeDbContextFactory<SistemaDeTarefaDBContext>
    {
        public SistemaDeTarefaDBContext CreateDbContext(string[] args)
        {
            
            var connectionString =
                "Host=ep-sparkling-hill-a4xzebk1-pooler.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_4rfXu2ZiBvEJ;SSL Mode=Require;Trust Server Certificate=true";

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            dataSourceBuilder.EnableUnmappedTypes();
            dataSourceBuilder.MapEnum<Status>("status_enum");
            dataSourceBuilder.MapEnum<Prioridade>("prioridade_enum");

            var dataSource = dataSourceBuilder.Build();

            var options = new DbContextOptionsBuilder<SistemaDeTarefaDBContext>()
                .UseNpgsql(dataSource)
                .Options;

            return new SistemaDeTarefaDBContext(options);
        }
    }
}
