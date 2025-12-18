using Newtonsoft.Json;
using SOMIOD.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SOMIOD.Utils
{
    public static class HttpHelper
    {
        // HttpClient deve ser estático e reutilizado para evitar exaustão de sockets
        private static readonly HttpClient client = new HttpClient();

        public static async Task SendNotificationAsync(NotificationModel notification, string endpointUrl)
        {
            try
            {
                // Serializar o objeto de notificação para String JSON
                string json = JsonConvert.SerializeObject(notification);

                // O envelope é "application/json", independentemente de o conteúdo interno ser XML ou Texto.
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Enviar o POST
                HttpResponseMessage response = await client.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[HttpHelper] Notification sent successfully to {endpointUrl}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[HttpHelper] Failed to send. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpHelper] Error: {ex.Message}");
            }
        }
    }
}