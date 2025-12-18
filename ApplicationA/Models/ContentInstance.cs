using Newtonsoft.Json;
using System;

namespace ApplicationA.Models
{
    public class ContentInstanceModel
    {
        // Ocultar ID interno 
        [JsonIgnore]
        public int Id { get; set; }

        // Mapeamento para "res-type"
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        // Mapeamento para "resource-name"
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        // Mapeamento para "creation-datetime"
        [JsonProperty("creation-datetime")]
        public DateTime CreationDateTime { get; set; }

        // O conteúdo em si
        [JsonProperty("content")]
        public string Content { get; set; }

        // Tipo de conteúdo
        [JsonProperty("content-type")]
        public string ContentType { get; set; }

        // Ocultar chave estrangeira
        [JsonIgnore]
        public int ParentId { get; set; }

        public ContentInstanceModel()
        {
            ResType = "content-instance";
            CreationDateTime = DateTime.Now;
        }
    }
}