using System;
using System.Configuration;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SOMIOD.Utils
{
    public static class MqttHelper
    {
        private static MqttClient client;

        // Lê do Web.config (chave "MqttBroker") ou usa localhost por defeito
        private static string BrokerIp => ConfigurationManager.AppSettings["MqttBroker"] ?? "127.0.0.1";

        // TORNAR PÚBLICO para ser chamado no Global.asax Application_Start
        public static void Connect()
        {
            try
            {
                if (client == null || !client.IsConnected)
                {
                    // Cria o cliente
                    client = new MqttClient(BrokerIp);
                    string clientId = Guid.NewGuid().ToString();

                    // Tenta conectar
                    client.Connect(clientId);

                    if (client.IsConnected)
                        System.Diagnostics.Debug.WriteLine($"[MQTT] Connected to {BrokerIp}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MQTT] Connection Failed: {ex.Message}");
                // Não lançamos a exceção para não mandar abaixo a API
            }
        }

        // NOVO MÉTODO: Necessário para o Global.asax Application_End
        public static void Disconnect()
        {
            try
            {
                if (client != null && client.IsConnected)
                {
                    client.Disconnect();
                    System.Diagnostics.Debug.WriteLine("[MQTT] Disconnected.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MQTT] Disconnect Error: {ex.Message}");
            }
        }

        public static void Publish(string topic, string payload)
        {
            try
            {
                // Garante conexão antes de enviar (caso a conexão tenha caído)
                if (client == null || !client.IsConnected)
                {
                    Connect();
                }

                if (client != null && client.IsConnected)
                {
                    // Envia com QoS 1 (At Least Once) para garantir entrega
                    client.Publish(topic, Encoding.UTF8.GetBytes(payload),
                                   MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);

                    System.Diagnostics.Debug.WriteLine($"[MQTT] Sent to {topic}: {payload}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MQTT] Publish Failed: {ex.Message}");
            }
        }
    }
}