using System;

namespace SOMIOD.Models
{
    public class ContainerModel
    {
        // Identificador interno (base de dados)
        public int id { get; set; }

        // O enunciado exige "res-type" com o valor "container"
        public string res_type { get; set; }

        // O nome único do recurso dentro da aplicação pai
        public string resource_name { get; set; }

        // Data de criação obrigatória
        public DateTime creation_datetime { get; set; }

        // Propriedade de navegação (útil para lógica interna)
        public int parent_id { get; set; }

        public ContainerModel()
        {
            res_type = "container";
            creation_datetime = DateTime.Now;
        }
    }
}