using Newtonsoft.Json;
using SOMIOD.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class HttpHelper
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task SendNotificationAsync(NotificationModel notification, string endpointUrl)
    {
        try
        {
            string json = JsonConvert.SerializeObject(notification);
            var content = new StringContent(json, Encoding.UTF8, notification.content_type);

            HttpResponseMessage response = await client.PostAsync(endpointUrl, content);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Notification sent successfully to {endpointUrl}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send notification. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
        }
    }
}