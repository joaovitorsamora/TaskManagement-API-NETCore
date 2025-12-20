using System.ComponentModel.DataAnnotations.Schema;

namespace GerenciadorDeTarefas.Models
{
    public class TarefaModel
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public DateTime DataCriacao { get; set; }

        [Column(TypeName = "status_enum")]
        public Status StatusTarefa { get; set; }

        [Column(TypeName = "prioridade_enum")]
        public Prioridade PrioridadeTarefa { get; set; }

        public int? ProjetoId { get; set; } 
        public virtual ProjetoModel? Projeto { get; set; }

        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; } = null!;

        public ICollection<TagModel> Tags { get; set; } = new List<TagModel>();
    }
}
