-- 1. Tabela APPLICATIONS
CREATE TABLE Applications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,       -- Corresponde a 'resource-name'
    CreationDate DATETIME DEFAULT GETDATE()  -- Corresponde a 'creation-datetime'
);

-- 2. Tabela CONTAINERS
CREATE TABLE Containers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,              -- Corresponde a 'resource-name'
    CreationDate DATETIME DEFAULT GETDATE(), -- Corresponde a 'creation-datetime'
    ParentAppId INT NOT NULL,
    FOREIGN KEY (ParentAppId) REFERENCES Applications(Id),
    UNIQUE(Name, ParentAppId)                -- O nome só tem de ser único dentro da mesma App
);

-- 3. Tabela CONTENTINSTANCES
-- Armazena os dados. Não permite updates, apenas create/delete.
CREATE TABLE ContentInstances (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,              -- Corresponde a 'resource-name'
    CreationDate DATETIME DEFAULT GETDATE(), -- Corresponde a 'creation-datetime'
    ContentType NVARCHAR(50) NOT NULL,       -- ex: 'application/json', 'application/xml'
    Content NVARCHAR(MAX) NOT NULL,          -- O conteúdo em si (JSON, XML ou texto)
    ParentContainerId INT NOT NULL,
    FOREIGN KEY (ParentContainerId) REFERENCES Containers(Id),
    UNIQUE(Name, ParentContainerId)
);

-- 4. Tabela SUBSCRIPTIONS
-- Gere as notificações (MQTT/HTTP).
CREATE TABLE Subscriptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,              -- Corresponde a 'resource-name'
    CreationDate DATETIME DEFAULT GETDATE(), -- Corresponde a 'creation-datetime'
    Event NVARCHAR(1) NOT NULL,              -- '1' (Creation) ou '2' (Deletion)
    Endpoint NVARCHAR(200) NOT NULL,         -- URL ou IP para notificação
    ParentContainerId INT NOT NULL,
    FOREIGN KEY (ParentContainerId) REFERENCES Containers(Id),
    UNIQUE(Name, ParentContainerId)
);