-- Script de inicialización para MySQL - Microservicio Reports
-- Este script se ejecuta automáticamente cuando se crea el contenedor

-- Crear usuario adicional para operaciones de solo lectura
CREATE USER IF NOT EXISTS 'readonly_user'@'%' IDENTIFIED BY 'ReadOnly2025ReportsPass';
GRANT SELECT ON reportsdb.* TO 'readonly_user'@'%';

-- Crear usuario para backup
CREATE USER IF NOT EXISTS 'backup_user'@'%' IDENTIFIED BY 'BackUp2025ReportsPass';
GRANT SELECT, LOCK TABLES, SHOW VIEW ON reportsdb.* TO 'backup_user'@'%';

-- Crear usuario para monitoreo
CREATE USER IF NOT EXISTS 'monitor_user'@'%' IDENTIFIED BY 'Monitor2025ReportsPass';
GRANT PROCESS ON *.* TO 'monitor_user'@'%';

-- Crear usuario para generación de reportes con permisos específicos
CREATE USER IF NOT EXISTS 'report_generator'@'%' IDENTIFIED BY 'RptGen2025Pass';
GRANT SELECT, INSERT ON reportsdb.* TO 'report_generator'@'%';

-- Reforzar permisos del usuario principal de la aplicación
GRANT ALL PRIVILEGES ON reportsdb.* TO 'msuser'@'%';

FLUSH PRIVILEGES;

-- Crear tabla de logs de reportes si no existe
USE reportsdb;
CREATE TABLE IF NOT EXISTS report_logs (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    report_id VARCHAR(255),
    report_type VARCHAR(100),
    status VARCHAR(50),
    generated_by VARCHAR(100),
    generation_time DATETIME,
    file_size_kb INT,
    export_format VARCHAR(20),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
