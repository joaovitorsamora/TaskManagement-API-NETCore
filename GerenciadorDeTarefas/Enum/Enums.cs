using System.Text.Json.Serialization;

namespace GerenciadorDeTarefas.Models
{
    
    public enum Prioridade
    {
        todas,
        alta,
        media,
        baixa
    }

    public enum Status
    {
        aberta,
        concluida
    }

}