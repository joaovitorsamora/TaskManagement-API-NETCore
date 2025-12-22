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
            modelBuilder.HasPostgresEnum<Prioridade>("prioridade_enum");
            modelBuilder.HasPostgresEnum<Status>("status_enum");

            // Adicione estas linhas para forçar as colunas a usarem o tipo enum
            modelBuilder.Entity<TarefaModel>()
                .Property(t => t.StatusTarefa)
                .HasColumnType("status_enum");

            modelBuilder.Entity<TarefaModel>()
                .Property(t => t.PrioridadeTarefa)
                .HasColumnType("prioridade_enum");

            base.OnModelCreating(modelBuilder);
        }





    }
}