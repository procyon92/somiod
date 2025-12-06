using System;
using System.Diagnostics;

namespace SOMIOD
{
    public static class MqttHelper
    {
        // Simula a conexão ao Mosquitto
        public static void Connect()
        {
            Debug.WriteLine("MQTT: Conectado (Simulação)");
        }

        // Simula a desconexão
        public static void Disconnect()
        {
            Debug.WriteLine("MQTT: Desconectado (Simulação)");
        }

        // Simula o envio de uma mensagem
        public static void Publish(string topic, string message)
        {
            Debug.WriteLine($"MQTT PUBLISH [{topic}]: {message}");
        }
    }
}