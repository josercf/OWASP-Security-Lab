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
-- para fins educacionais de demonstração de SQL Injection.
-- NUNCA faça isso em produção!
CREATE TABLE "Users" (
    "Id" VARCHAR(120) NOT NULL,
    "Name" VARCHAR(120) NOT NULL,
    "Password" VARCHAR(120) NOT NULL,
    PRIMARY KEY ("Id")
);

-- Inserir usuários de exemplo
INSERT INTO "Users" ("Id", "Name", "Password") VALUES
    ('1', 'admin', 'admin123'),
    ('2', 'john.doe', 'password123'),
    ('3', 'jane.smith', 'secret456'),
    ('4', 'bob.wilson', 'qwerty789');

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
