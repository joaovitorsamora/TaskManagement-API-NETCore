using GerenciadorDeTarefas.Models;

namespace GerenciadorDeTarefas.DTOs
{
    public class TarefaDTO
    {
        
        public int? Id { get; set; }
        public int? ProjetoId { get; set; }
        public int UsuarioId { get; set; }

        
        public string? Titulo { get; set; }
        public DateTime DataCriacao { get; set; }
        public string? ProjetoNome { get; set; }

        
        public Status StatusTarefa { get; set; }
        public Prioridade PrioridadeTarefa { get; set; }
        public List<string>? Tags { get; set; }
    }
}
