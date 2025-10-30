-- ========================================
-- OWASP Security Lab - Database Setup
-- ========================================
-- Este script inicializa o banco de dados para o laboratório
-- de segurança com dados de exemplo.
--
-- IMPORTANTE: Este script é executado automaticamente pelo PostgreSQL
-- quando o container é iniciado pela primeira vez. O banco 'vulnerabledb'
-- já é criado automaticamente via POSTGRES_DB no docker-compose.yml

-- Criar usuário da aplicação
CREATE USER vulnapp WITH ENCRYPTED PASSWORD 'password123';

-- Garantir permissões no schema public
GRANT ALL ON SCHEMA public TO vulnapp;
GRANT ALL ON DATABASE vulnerabledb TO vulnapp;

-- ========================================
-- Tabela: Product
-- ========================================
CREATE TABLE "Product" (
    "Id" VARCHAR(120) NOT NULL,
    "Name" VARCHAR(120) NOT NULL,
    "Price" VARCHAR(120) NOT NULL,
    PRIMARY KEY ("Id")
);

-- Inserir produtos de exemplo
INSERT INTO "Product" ("Id", "Name", "Price") VALUES
    ('1', 'Hammer', '12.30'),
    ('2', 'Driver-Drills', '16.84'),
    ('3', 'Hammer Drills', '22.30'),
    ('4', 'Rotary Drills', '109.95'),
    ('5', 'Screwdriver Set', '29.90'),
    ('6', 'Wrench Set', '45.50'),
    ('7', 'Power Saw', '199.99'),
    ('8', 'Measuring Tape', '8.75');

-- ========================================
-- Tabela: Users
-- ========================================
-- ⚠️ ATENÇÃO: Esta tabela contém senhas em texto plano
-- para fins educacionais de demonstração de SQL Injection
-- e Broken Access Control.
-- NUNCA faça isso em produção!
CREATE TABLE "Users" (
    "Id" VARCHAR(120) NOT NULL,
    "Name" VARCHAR(120) NOT NULL,
    "Email" VARCHAR(120) NOT NULL,
    "Password" VARCHAR(120) NOT NULL,
    "Role" VARCHAR(50) NOT NULL DEFAULT 'User',
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    PRIMARY KEY ("Id")
);

-- Inserir usuários de exemplo
-- 1 Admin e 3 usuários comuns para demonstração de Broken Access Control
INSERT INTO "Users" ("Id", "Name", "Email", "Password", "Role", "IsActive") VALUES
    ('1', 'Administrator', 'admin@owasp-lab.local', 'admin123', 'Admin', true),
    ('2', 'John Doe', 'john.doe@owasp-lab.local', 'password123', 'User', true),
    ('3', 'Jane Smith', 'jane.smith@owasp-lab.local', 'secret456', 'User', true),
    ('4', 'Bob Wilson', 'bob.wilson@owasp-lab.local', 'qwerty789', 'User', true),
    ('5', 'Alice Johnson', 'alice.johnson@owasp-lab.local', 'pass9999', 'User', false);

-- ========================================
-- Tabela: AuditLog
-- ========================================
-- Para demonstração de Security Logging and Monitoring Failures
CREATE TABLE "AuditLog" (
    "Id" VARCHAR(120) NOT NULL,
    "Timestamp" TIMESTAMP NOT NULL,
    "Level" VARCHAR(50) NOT NULL,
    "EventType" VARCHAR(100) NOT NULL,
    "Username" VARCHAR(120),
    "IpAddress" VARCHAR(50),
    "Action" VARCHAR(200),
    "Resource" VARCHAR(200),
    "Success" BOOLEAN NOT NULL,
    "Message" TEXT,
    "Details" TEXT,
    PRIMARY KEY ("Id")
);

-- Criar índices para melhor performance em consultas
CREATE INDEX idx_auditlog_timestamp ON "AuditLog"("Timestamp" DESC);
CREATE INDEX idx_auditlog_username ON "AuditLog"("Username");
CREATE INDEX idx_auditlog_eventtype ON "AuditLog"("EventType");

-- ========================================
-- Permissões
-- ========================================
-- Conceder permissões ao usuário da aplicação
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO vulnapp;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO vulnapp;

-- ========================================
-- Logs e Informações
-- ========================================
-- Exibir resumo
SELECT 'Database setup completed!' as status;
SELECT COUNT(*) as total_products FROM "Product";
SELECT COUNT(*) as total_users FROM "Users";
