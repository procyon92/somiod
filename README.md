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

## 4. Resolução de Problemas (Troubleshooting)

### Projeto com ficheiros em falta (X Vermelho) ou dessincronizado
Se ao baixar o projeto do repositório, o Visual Studio apresentar um **X vermelho** em ficheiros (ex: `Form1.cs`) ou o projeto parecer bloqueado:

1.  **Remover a referência quebrada:**
    * No *Solution Explorer*, clica com o botão direito no projeto problemático (ex: `ApplicationB`).
    * Escolhe a opção **Remove**. (Isto remove apenas a visualização no VS, não apaga os ficheiros).
2.  **Re-adicionar o Projeto:**
    * Clica com o botão direito na **Solution 'somiod'** (no topo).
    * Vai a **Add** > **Existing Project...**.
    * Entra na pasta do projeto e seleciona o ficheiro `.csproj` correto (ex: `ApplicationB.csproj`).
3.  **Corrigir ficheiros renomeados:**
    * Se ainda houver ficheiros com X vermelho (ex: `Form1.cs`), apaga-os do Solution Explorer (**Delete**).
    * Clica com o botão direito no Projeto > **Add** > **Existing Item...**.
    * Seleciona os ficheiros renomeados (ex: `ApplicationB.cs`, `ApplicationB.Designer.cs`, `ApplicationB.resx`) e adiciona-os.
4.  **Verificar o Ponto de Entrada:**
    * Abre o ficheiro `Program.cs`.
    * Garante que o método `Application.Run(...)` está a iniciar o formulário com o nome correto (ex: substituir `new Form1()` por `new ApplicationB()`).