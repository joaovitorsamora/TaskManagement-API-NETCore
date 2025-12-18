using System.Text.Json.Serialization;


public enum Prioridade 
{ 
    Todas = 1,
    Alta = 2,
    Media = 3,
    Baixa = 4
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status 
{ 
    Aberta,
    Concluida
}