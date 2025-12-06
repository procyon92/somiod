using System;
using Newtonsoft.Json;

namespace ApplicationA.Models
{
    public class ContentInstanceModel
    {
        // ID interno da Base de Dados
        public int id { get; set; }

        // Nota: Em C# não podemos usar hífen no nome da variável como "content-type", por exemplo  
        // por isso resolvemos usar JsonProperty para mapear corretamente.

        // Deve ser sempre "content-instance" 
        [JsonProperty("res-type")]
        public string res_type { get; set; }

        // Nome único dentro do contentor pai
        [JsonProperty("resource-name")]
        public string resource_name { get; set; }

        // Data de criação
        [JsonProperty("creation-datetime")]
        public DateTime creation_datetime { get; set; }

        // O conteúdo em si (pode ser XML, JSON ou texto simples) 
        public string content { get; set; }

        // O tipo de conteúdo (ex: "application/json", "application/xml") 
        [JsonProperty("content-type")]
        public string content_type { get; set; }

        // ID do Contentor a que este dado pertence
        public int parent_id { get; set; }

        public ContentInstanceModel()
        {
            res_type = "content-instance";
            creation_datetime = DateTime.Now;
        }
    }
}