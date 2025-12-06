using System;
using System.Threading.Tasks;
using SOMIOD.Models; // Necessário para conhecer o NotificationModel

namespace SOMIOD
{
    public static class HttpHelper
    {
        // Simula o envio de notificação HTTP (fire-and-forget)
        public static async Task SendNotificationAsync(NotificationModel notification, string endpoint)
        {
            // Aqui futuramente fará o POST real. 
            // Para já, apenas finge que envia para o projeto compilar.
            await Task.Run(() =>
            {
                System.Diagnostics.Debug.WriteLine($"HTTP POST para {endpoint}: Evento {notification.evt}");
            });
        }
    }
}