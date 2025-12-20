using GerenciadorDeTarefas.Models;
using Microsoft.EntityFrameworkCore;

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
            // Registra os Enums do Postgres
            modelBuilder.HasPostgresEnum<Status>("status_enum");
            modelBuilder.HasPostgresEnum<Prioridade>("prioridade_enum");

            // CONFIGURAÇÃO MANUAL DAS COLUNAS DA TABELA TAREFAS
            modelBuilder.Entity<TarefaModel>(entity =>
            {
                entity.Property(e => e.StatusTarefa)
                    .HasColumnType("status_enum");

                entity.Property(e => e.PrioridadeTarefa)
                    .HasColumnType("prioridade_enum");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}