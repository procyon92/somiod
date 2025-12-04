# Projeto SOMIOD

## Pré-requisitos

* Visual Studio (com workload ASP.NET e desenvolvimento Web).
* Mosquitto MQTT Broker instalado e a correr.

## 1. Como configurar a Base de Dados

1. No Visual Studio, abrir a pasta `App_Data` (Solution Explorer).
2. Criar uma base de dados SQL Server chamada `SOMIOD.mdf`.
3. Abrir o ficheiro e executar o script SQL que está na pasta `Data/Script.sql`.

## 2. Como configurar o MQTT

1. Abrir a linha de comandos (CMD/PowerShell).
2. Iniciar o Mosquitto (se não for serviço): `mosquitto -v`.
3. (Opcional) Para ver as mensagens: `mosquitto_sub -t "api/somiod/#" -v`.

## 3. Como configurar o POSTMAN

1. Carregar em **Import** no Postman.
2. Selecionar o ficheiro `.json` da coleção (na pasta Project/Other?).
3. Nas variáveis da coleção, substituir a variável `{{url}}` pelo endereço local.
   * **Nota:** Verifica nas propriedades do teu projeto no Visual Studio qual é o porto atribuído (ex: `http://localhost:51364/api/somiod`).
