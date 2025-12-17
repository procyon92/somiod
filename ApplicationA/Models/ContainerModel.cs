using Newtonsoft.Json;
using System;

namespace ApplicationA.Models
{
    public class ContainerModel
    {
        // Ocultar dados internos 
        [JsonIgnore]
        public int Id { get; set; }

        // Mapeamento correto para JSON com hífen
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        // Mapeamento correto 
        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        // Mapeamento correto 
        [JsonProperty("creation-datetime")]
        public DateTime CreationDateTime { get; set; }

        // Ocultar chaves estrangeiras/hierarquia do utilizador final
        [JsonIgnore]
        public int ParentId { get; set; }

        public ContainerModel()
        {
            ResType = "container";
            CreationDateTime = DateTime.Now;
        }
    }
}