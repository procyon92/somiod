using Newtonsoft.Json;

namespace ApplicationB.Models
{
    public class NotificationModel
    {
        // Define se foi "creation" ou "deletion"
        [JsonProperty("event-type")]
        public string EventType { get; set; }

        // O nome do recurso criado 
        [JsonProperty("resource")]
        public string ResourceName { get; set; }

        // O contentor pai 
        [JsonProperty("container")]
        public string ContainerName { get; set; }

        // A aplicação pai - Útil para o cliente filtrar
        [JsonProperty("app")]
        public string AppName { get; set; }

        // Data e hora do evento
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        // O conteúdo XML ou JSON (apenas se for creation, senão vem null)
        [JsonProperty("content")]
        public string Content { get; set; }

        // O tipo do conteúdo 
        [JsonProperty("content-type")]
        public string ContentType { get; set; }
    }
}