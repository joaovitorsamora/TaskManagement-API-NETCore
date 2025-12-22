namespace GerenciadorDeTarefas.Models
{
    public class TarefaModel
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public DateTime DataCriacao { get; set; }

        public Status StatusTarefa { get; set; }
        public Prioridade PrioridadeTarefa { get; set; }

        public int? ProjetoId { get; set; }
        public ProjetoModel? Projeto { get; set; }

        public int UsuarioId { get; set; }
        public UsuarioModel Usuario { get; set; } = null!;

        public ICollection<TagModel> Tags { get; set; } = new List<TagModel>();
    }
}
