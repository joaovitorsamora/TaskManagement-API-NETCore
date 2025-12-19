using System.Text.Json.Serialization;

namespace GerenciadorDeTarefas.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Prioridade
    {
        Todas,
        Alta,
        Media,
        Baixa
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Status
    {
        Aberta,
        Concluida
    }
}