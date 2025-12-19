using GerenciadorDeTarefas.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace GerenciadorDeTarefas.Data
{
    public class SistemaDeTarefaDBContext : DbContext
    {
        public DbSet<UsuarioModel> Usuarios { get; set; }
        public DbSet<ProjetoModel> Projetos { get; set; }
        public DbSet<TarefaModel> Tarefas { get; set; }
        public DbSet<TagModel> Tags { get; set; }

        public SistemaDeTarefaDBContext(DbContextOptions<SistemaDeTarefaDBContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum(
                schema: null,
                name: "status_enum",
                labels: new[] { "Aberta", "Concluida" }
            );

            modelBuilder.HasPostgresEnum(
                schema: null,
                name: "prioridade_enum",
                labels: new[] { "Todas", "Alta", "Media", "Baixa" }
            );

            base.OnModelCreating(modelBuilder);
        }


    }
}
