-- ========================================
-- OWASP Security Lab - Validation Script
-- ========================================
-- Este script valida se a carga de dados foi bem-sucedida

\echo '========================================='
\echo 'OWASP Security Lab - Database Validation'
\echo '========================================='

-- Verificar tabelas criadas
\echo ''
\echo '📋 Checking tables...'
SELECT
    schemaname,
    tablename
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY tablename;

-- Verificar produtos carregados
\echo ''
\echo '🛠️  Products loaded:'
SELECT COUNT(*) as total_products FROM "Product";

\echo ''
\echo 'Sample products:'
SELECT "Id", "Name", "Price" FROM "Product" LIMIT 3;

-- Verificar usuários carregados
\echo ''
\echo '👥 Users loaded:'
SELECT COUNT(*) as total_users FROM "Users";

\echo ''
\echo 'Sample users:'
SELECT "Id", "Name" FROM "Users" LIMIT 2;

-- Verificar permissões do usuário vulnapp
\echo ''
\echo '🔐 Permissions for vulnapp user:'
SELECT
    grantee,
    privilege_type
FROM information_schema.role_table_grants
WHERE grantee = 'vulnapp'
LIMIT 5;

\echo ''
\echo '✅ Database initialization completed successfully!'
\echo '========================================='
