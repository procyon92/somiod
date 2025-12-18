# SOMIOD - Service Oriented Middleware for IOT Devices

Este projeto implementa um Middleware RESTful para a gestão de dispositivos IoT, permitindo a criação de aplicações, contentores e subscrições de eventos através de protocolos HTTP e MQTT.

## 📋 Pré-requisitos

Para executar este projeto, garante que tens o seguinte software instalado:

* **Visual Studio 2019/2022** (com as workloads *"ASP.NET and Web Development"* e *.NET Desktop Development*).
* **SQL Server** (LocalDB ou Express).
* **Mosquitto MQTT Broker** (instalado e configurado no PATH ou como serviço).
* **Postman** (opcional, para testes manuais da API).

---

## ⚙️ Instalação e Configuração

Como as bibliotecas externas não estão incluídas no repositório (via `.gitignore`), é necessário seguir estes passos para configurar o ambiente.

### 1. Clonar e Restaurar Pacotes
1.  Clona este repositório para a tua máquina local.
2.  Abre o ficheiro `somiod.sln` no Visual Studio.
3.  **Restaurar Pacotes NuGet:**
    * No *Solution Explorer*, clica com o botão direito sobre a **Solution 'somiod'**.
    * Seleciona a opção **Restore NuGet Packages**.
    * Aguarda que todas as dependências (M2Mqtt, Swagger, etc.) sejam descarregadas.

### 2. Configurar a Base de Dados
1.  No Visual Studio, navega até à pasta `App_Data` do projeto **SOMIOD**.
2.  Cria uma nova Base de Dados SQL Server chamada `SOMIOD.mdf` (se não existir).
3.  Abre o ficheiro de script `Data/Script.sql`.
4.  Executa o script nessa base de dados para criar as tabelas (`Applications`, `Containers`, `Subscriptions`, `ContentInstances`, etc.).
5.  **Verificação:** Confirma no `Web.config` se a *ConnectionString* `SomiodDB` aponta corretamente para o caminho do teu ficheiro `.mdf`.

### 3. Configurar o MQTT Broker
O middleware utiliza o MQTT para notificar as aplicações cliente.
1.  Abre a linha de comandos (CMD ou PowerShell).
2.  Inicia o broker (modo verbose para debug):
    ```powershell
    mosquitto -v
    ```
    *(Nota: Se tiveres o Mosquitto como Serviço do Windows, certifica-te apenas que está a correr).*

---

## 🚀 Como Executar o Projeto

A solução é composta pelo Middleware (API) e pelas Aplicações Cliente (App A e App B). Deves iniciar a API primeiro.

### Passo 1: Iniciar o Middleware (API)
1.  Define o projeto **SOMIOD** como o projeto de arranque (*Set as Startup Project*).
2.  Pressiona `F5` ou clica em **Start**.
3.  O browser abrirá automaticamente. Navega para a documentação Swagger para confirmar que a API está online:
    * URL: `http://localhost:<SEU_PORTO>/swagger`

### Passo 2: Iniciar as Aplicações Cliente
1.  No *Solution Explorer*, clica com o botão direito no projeto **ApplicationA** (ou **ApplicationB**).
2.  Escolhe **Debug** > **Start New Instance**.
3.  A aplicação Windows Forms irá abrir e conectar-se à API e ao Broker MQTT.

---

## 🧪 Testes e Documentação

### Swagger UI
A API está totalmente documentada via Swagger. Podes testar todos os endpoints (GET, POST, PUT, DELETE) e visualizar os modelos de dados diretamente no browser em `/swagger`.

### Postman
Para testes de integração mais complexos:
1.  No Postman, clica em **Import**.
2.  Seleciona o ficheiro `.json` da coleção (localizado na pasta `Project/Docs` ou similar).
3.  Nas variáveis da coleção, atualiza a variável `{{url}}` com o teu porto local (ex: `http://localhost:51364/api/somiod`).

---

## 🔧 Resolução de Problemas (Troubleshooting)

### Projeto dessincronizado ou ficheiros com "X Vermelho"
Se, após baixar o projeto, visualizares um ícone de **X vermelho** em ficheiros (como `Form1.cs` ou `ApplicationB.cs`) ou o projeto não compilar devido a ficheiros em falta:

1.  **Remover a referência quebrada:**
    * Clica com o botão direito no projeto afetado (ex: `ApplicationB`).
    * Escolhe **Remove** (isto remove o projeto da solução, não apaga ficheiros do disco).
2.  **Re-adicionar o Projeto:**
    * Clica com o botão direito na **Solution 'somiod'**.
    * Escolhe **Add** > **Existing Project...**.
    * Navega até à pasta do projeto e seleciona o ficheiro `.csproj` correspondente.
3.  **Corrigir ficheiros renomeados (se necessário):**
    * Se o erro persistir em ficheiros específicos, apaga a referência quebrada no *Solution Explorer* (tecla Del).
    * Clica com o botão direito no Projeto > **Add** > **Existing Item...**.
    * Seleciona os ficheiros corretos (ex: `ApplicationB.cs`, `ApplicationB.Designer.cs`, `ApplicationB.resx`).
4.  **Verificar o Ponto de Entrada (`Program.cs`):**
    * Abre o `Program.cs` e garante que o método `Application.Run(new ApplicationB());` instancia o formulário correto.

### Erro "DLL Hell" / Microsoft.Web.Infrastructure
Se ao correr a API obtiveres um erro sobre versões de assembly:
1.  Abre a **Package Manager Console** (Tools > NuGet Package Manager).
2.  Executa: `Update-Package -Reinstall Microsoft.Web.Infrastructure`.
3.  Verifica se o `Web.config` tem o *bindingRedirect* correto para a versão instalada.

---

**Autores:** André Clérigo, Ian Rosales, Maria Martins, Joaquim Pereira
**Unidade Curricular:** Integração de Sistemas - 2025