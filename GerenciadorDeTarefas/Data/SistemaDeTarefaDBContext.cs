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
            modelBuilder.Entity<TarefaModel>(entity =>
            {
                entity.Property(t => t.StatusTarefa)
                    .HasConversion<string>() 
                    .HasMaxLength(20); 

                entity.Property(t => t.PrioridadeTarefa)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}