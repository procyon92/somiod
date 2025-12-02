using System;
using Newtonsoft.Json;

namespace SOMIOD.Models
{
    public class SubscriptionModel
    {
        // ID interno da Base de Dados
        public int id { get; set; }

        // Deve ser sempre "subscription"
        public string res_type { get; set; }

        // Nome único dentro do contentor pai
        public string resource_name { get; set; }

        // Data de criação
        public DateTime creation_datetime { get; set; }

        // Evento que despoleta a notificação:
        // '1' = Criação (Creation)
        // '2' = Apagar (Deletion)
        // Nota: No enunciado é int, mas na Base de Dados definimos como NVARCHAR(1)
        // para simplificar, vamos tratar como string na API também.
        public string evt { get; set; }

        // O endereço onde a notificação será enviada (MQTT ou HTTP)
        public string endpoint { get; set; }

        // ID do Contentor a que esta subscrição pertence
        public int parent_id { get; set; }

        public SubscriptionModel()
        {
            res_type = "subscription";
            creation_datetime = DateTime.Now;
        }
    }
}