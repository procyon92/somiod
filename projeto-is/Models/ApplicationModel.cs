using Newtonsoft.Json;
using System;

namespace SOMIOD.Models
{
    public class ApplicationModel
    {
        // O 'id' deve ser público para o teu código aceder, 
        // mas deve ter [JsonIgnore] para não ser enviado na API.
        [JsonIgnore]
        public int Id { get; set; }

        // Usamos [JsonProperty] para mapear a propriedade C# para o nome JSON com hífen exigido.
        [JsonProperty("res-type")]
        public string ResType { get; set; }

        [JsonProperty("resource-name")]
        public string ResourceName { get; set; }

        [JsonProperty("creation-datetime")]
        public DateTime CreationDateTime { get; set; }

        public ApplicationModel()
        {
            ResType = "application";
            CreationDateTime = DateTime.Now;
        }
    }
}