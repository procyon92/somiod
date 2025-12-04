using System;
using System.Text;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MqttHelper
{
    private static MqttClient client;

    public static void Connect()
    {
        if (client != null && client.IsConnected)
        {
            return;
        }

        client = new MqttClient("127.0.0.1");
        client.MqttMsgPublishReceived += OnMessageReceived;

        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        if (client.IsConnected)
            System.Diagnostics.Debug.WriteLine("[MQTT] Connected to broker.");
        else
            System.Diagnostics.Debug.WriteLine("[MQTT] Failed to connect to broker.");
    }

    public static void Publish(string topic, string payload)
    {
        if (client == null || !client.IsConnected)
            Connect();

        client.Publish(topic, Encoding.UTF8.GetBytes(payload),
                       MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
        System.Diagnostics.Debug.WriteLine($"[MQTT] Published '{payload}' to '{topic}'");
    }

    private static void OnMessageReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string msg = Encoding.UTF8.GetString(e.Message);
        System.Diagnostics.Debug.WriteLine($"[MQTT] Message received: {msg} on {e.Topic}");
    }

    public static void Disconnect()
    {
        if (client != null && client.IsConnected)
        {
            client.Disconnect();
            System.Diagnostics.Debug.WriteLine("[MQTT] Disconnected.");
        }
    }
}
