using System;

namespace SOMIOD.Models
{
    public class ContentInstanceModel
    {
        // ID interno da Base de Dados
        public int id { get; set; }

        // Deve ser sempre "content-instance" 
        public string res_type { get; set; }

        // Nome único dentro do contentor pai
        public string resource_name { get; set; }

        // Data de criação
        public DateTime creation_datetime { get; set; }

        // O conteúdo em si (pode ser XML, JSON ou texto simples) 
        public string content { get; set; }

        // O tipo de conteúdo (ex: "application/json", "application/xml") 
        // Nota: Em C# não podemos usar hífen no nome da variável ("content-type"), 
        // por isso usamos underscore.
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