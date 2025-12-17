-- 0. SEGURANÇA: Apagar tabelas se existirem (pela ordem inversa das FK)
IF OBJECT_ID('dbo.Subscriptions', 'U') IS NOT NULL DROP TABLE dbo.Subscriptions;
IF OBJECT_ID('dbo.ContentInstances', 'U') IS NOT NULL DROP TABLE dbo.ContentInstances;
IF OBJECT_ID('dbo.Containers', 'U') IS NOT NULL DROP TABLE dbo.Containers;
IF OBJECT_ID('dbo.Applications', 'U') IS NOT NULL DROP TABLE dbo.Applications;

-- 1. Tabela APPLICATIONS
CREATE TABLE Applications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,       -- resource-name
    CreationDate DATETIME DEFAULT GETDATE()  -- creation-datetime
);

-- 2. Tabela CONTAINERS
CREATE TABLE Containers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,              -- resource-name
    CreationDate DATETIME DEFAULT GETDATE(), -- creation-datetime
    ParentAppId INT NOT NULL,
    FOREIGN KEY (ParentAppId) REFERENCES Applications(Id),
    UNIQUE(Name, ParentAppId)                -- Nome único dentro da App
);

-- 3. Tabela CONTENTINSTANCES
CREATE TABLE ContentInstances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,              -- resource-name
    CreationDate DATETIME DEFAULT GETDATE(), -- creation-datetime
    ContentType NVARCHAR(50) NOT NULL,       -- ex: 'application/xml'
    Content NVARCHAR(MAX) NOT NULL,          -- XML/JSON/Texto
    ParentContainerId INT NOT NULL,
    FOREIGN KEY (ParentContainerId) REFERENCES Containers(Id),
    UNIQUE(Name, ParentContainerId)
);

-- 4. Tabela SUBSCRIPTIONS
CREATE TABLE Subscriptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,              -- resource-name
    CreationDate DATETIME DEFAULT GETDATE(), -- creation-datetime
    Event INT NOT NULL CHECK (Event IN (1, 2)), 
    Endpoint NVARCHAR(200) NOT NULL,         -- ex: mqtt://127.0.0.1:1883
    ParentContainerId INT NOT NULL,
    FOREIGN KEY (ParentContainerId) REFERENCES Containers(Id),
    UNIQUE(Name, ParentContainerId)
);

-- 5. DUMMY DATA (Cenário Smart Parking)

-- Criar a Aplicação
INSERT INTO Applications (Name, CreationDate) VALUES ('smart-parking', GETDATE());

-- Guardar o ID da App numa variável 
DECLARE @AppId INT = (SELECT Id FROM Applications WHERE Name = 'smart-parking');

-- Criar o Container 'piso-01'
INSERT INTO Containers (Name, CreationDate, ParentAppId) VALUES ('piso-01', GETDATE(), @AppId);

-- Guardar o ID do Container
DECLARE @ContId INT = (SELECT Id FROM Containers WHERE Name = 'piso-01' AND ParentAppId = @AppId);

-- Inserir ContentInstance
INSERT INTO ContentInstances (Name, CreationDate, ContentType, Content, ParentContainerId)
VALUES ('record-init', GETDATE(), 'application/xml', '<parking><spot>A1</spot><status>occupied</status></parking>', @ContId);
