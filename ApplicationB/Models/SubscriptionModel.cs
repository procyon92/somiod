using Newtonsoft.Json;
using System;

namespace ApplicationB.Models
{
    public class SubscriptionModel
    {
        // Ocultar dados internos
        [JsonIgnore]
        public int Id { get; set; }

        // Mapeamento "res-type"
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        // Mapeamento "resource-name"
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        // Mapeamento "creation-datetime"
        [JsonProperty("creation-datetime")]
        public DateTime CreationDateTime { get; set; }

        // 'int' (1 ou 2).
        [JsonProperty("evt")]
        public int Evt { get; set; }

        // Endpoint (mqtt://... ou http://...)
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        // Ocultar chave estrangeira
        [JsonIgnore]
        public int ParentId { get; set; }

        public SubscriptionModel()
        {
            ResType = "subscription";
            CreationDateTime = DateTime.Now;
        }
    }
}