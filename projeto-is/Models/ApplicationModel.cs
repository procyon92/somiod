using System;
using Newtonsoft.Json;

namespace SOMIOD.Models
{
    public class ApplicationModel
    {
        public int id { get; set; } // Usado internamente, não precisa de ser enviado pelo utilizador

        // O enunciado exige estes nomes exatos no JSON:
        public string res_type { get; set; }
        public string resource_name { get; set; }
        public DateTime creation_datetime { get; set; }

        // Construtor para definir valores por defeito
        public ApplicationModel()
        {
            res_type = "application";
            creation_datetime = DateTime.Now;
        }
    }
}