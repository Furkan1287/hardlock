-- HARDLOCK Database Initialization Script

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Create schemas
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS storage;
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS access_control;

-- Identity Service Tables
CREATE TABLE IF NOT EXISTS identity.users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    requires_mfa BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_login_at TIMESTAMP WITH TIME ZONE,
    roles JSONB DEFAULT '["User"]',
    metadata JSONB DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS identity.refresh_tokens (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    token VARCHAR(500) UNIQUE NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_revoked BOOLEAN DEFAULT false
);

-- Storage Service Tables
CREATE TABLE IF NOT EXISTS storage.files (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    owner_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    file_name VARCHAR(255) NOT NULL,
    original_file_name VARCHAR(255),
    content_type VARCHAR(100) DEFAULT 'application/octet-stream',
    file_size BIGINT NOT NULL,
    file_hash VARCHAR(64) NOT NULL,
    encryption_hash VARCHAR(64) NOT NULL,
    -- File integrity fields
    integrity_hash VARCHAR(255),
    hash_algorithm VARCHAR(20) DEFAULT 'SHA256',
    integrity_verified BOOLEAN DEFAULT FALSE,
    last_integrity_check TIMESTAMP WITH TIME ZONE,
    integrity_check_count INTEGER DEFAULT 0,
    hash_verification_enabled BOOLEAN DEFAULT TRUE,
    is_encrypted BOOLEAN DEFAULT true,
    is_sharded BOOLEAN DEFAULT false,
    shard_count INTEGER DEFAULT 1,
    expires_at TIMESTAMP WITH TIME ZONE,
    unlock_at TIMESTAMP WITH TIME ZONE,
    -- Geo-fencing fields
    geo_fencing_enabled BOOLEAN DEFAULT false,
    allowed_regions GEOGRAPHY(POLYGON),
    allowed_countries TEXT[],
    allowed_cities TEXT[],
    allowed_location_lat DOUBLE PRECISION,
    allowed_location_lng DOUBLE PRECISION,
    allowed_radius DOUBLE PRECISION,
    -- Timelock fields
    has_timelock BOOLEAN DEFAULT false,
    timelock_block_number BIGINT,
    timelock_encrypted_key TEXT,
    timelock_public_key TEXT,
    timelock_type VARCHAR(20) DEFAULT 'none',
    -- Darknet backup fields
    darknet_backup_enabled BOOLEAN DEFAULT false,
    darknet_dht_hash TEXT,
    darknet_content_hashes TEXT[],
    -- Other security fields
    self_destruct_enabled BOOLEAN DEFAULT true,
    max_access_attempts INTEGER DEFAULT 3,
    current_access_attempts INTEGER DEFAULT 0,
    status VARCHAR(20) DEFAULT 'Active',
    access_level VARCHAR(20) DEFAULT 'Private',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    last_accessed_at TIMESTAMP WITH TIME ZONE,
    last_modified_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    metadata JSONB DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS storage.file_access (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_id UUID NOT NULL REFERENCES storage.files(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    permission VARCHAR(20) NOT NULL,
    granted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT true,
    UNIQUE(file_id, user_id)
);

CREATE TABLE IF NOT EXISTS storage.file_shards (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_id UUID NOT NULL REFERENCES storage.files(id) ON DELETE CASCADE,
    shard_index INTEGER NOT NULL,
    shard_hash VARCHAR(64) NOT NULL,
    encrypted_data JSONB NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(file_id, shard_index)
);

-- Audit Service Tables
CREATE TABLE IF NOT EXISTS audit.audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES identity.users(id),
    user_email VARCHAR(255),
    service VARCHAR(50) NOT NULL,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(50),
    resource_id VARCHAR(100),
    severity VARCHAR(20) DEFAULT 'Info',
    status VARCHAR(20) DEFAULT 'Success',
    ip_address INET,
    user_agent TEXT,
    location_lat DOUBLE PRECISION,
    location_lng DOUBLE PRECISION,
    request_data JSONB,
    response_data JSONB,
    error_message TEXT,
    metadata JSONB DEFAULT '{}',
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    duration_ms INTEGER
);

CREATE TABLE IF NOT EXISTS audit.security_events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES identity.users(id),
    user_email VARCHAR(255),
    event_type VARCHAR(50) NOT NULL,
    severity VARCHAR(20) DEFAULT 'Medium',
    file_id UUID REFERENCES storage.files(id),
    ip_address INET,
    user_agent TEXT,
    location_lat DOUBLE PRECISION,
    location_lng DOUBLE PRECISION,
    description TEXT NOT NULL,
    details JSONB DEFAULT '{}',
    requires_action BOOLEAN DEFAULT false,
    recommended_action VARCHAR(50),
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_resolved BOOLEAN DEFAULT false,
    resolved_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS audit.access_attempts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    file_id UUID NOT NULL REFERENCES storage.files(id) ON DELETE CASCADE,
    user_id UUID REFERENCES identity.users(id),
    user_email VARCHAR(255),
    is_successful BOOLEAN NOT NULL,
    failure_reason TEXT,
    ip_address INET,
    user_agent TEXT,
    location_lat DOUBLE PRECISION,
    location_lng DOUBLE PRECISION,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Access Control Service Tables
CREATE TABLE IF NOT EXISTS access_control.permissions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) UNIQUE NOT NULL,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS access_control.role_permissions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_name VARCHAR(100) NOT NULL,
    permission_id UUID NOT NULL REFERENCES access_control.permissions(id) ON DELETE CASCADE,
    granted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(role_name, permission_id)
);

CREATE TABLE IF NOT EXISTS access_control.user_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES identity.users(id) ON DELETE CASCADE,
    role_name VARCHAR(100) NOT NULL,
    granted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    granted_by UUID REFERENCES identity.users(id),
    UNIQUE(user_id, role_name)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_users_email ON identity.users(email);
CREATE INDEX IF NOT EXISTS idx_users_active ON identity.users(is_active);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON identity.refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON identity.refresh_tokens(token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires ON identity.refresh_tokens(expires_at);

CREATE INDEX IF NOT EXISTS idx_files_owner_id ON storage.files(owner_id);
CREATE INDEX IF NOT EXISTS idx_files_hash ON storage.files(file_hash);
CREATE INDEX IF NOT EXISTS idx_files_status ON storage.files(status);
CREATE INDEX IF NOT EXISTS idx_files_expires ON storage.files(expires_at);
CREATE INDEX IF NOT EXISTS idx_files_geo_fencing ON storage.files USING GIST(allowed_regions);
CREATE INDEX IF NOT EXISTS idx_files_geo_fencing_enabled ON storage.files(geo_fencing_enabled);
CREATE INDEX IF NOT EXISTS idx_files_timelock ON storage.files(has_timelock);
CREATE INDEX IF NOT EXISTS idx_files_darknet_backup ON storage.files(darknet_backup_enabled);
CREATE INDEX IF NOT EXISTS idx_file_access_file_id ON storage.file_access(file_id);
CREATE INDEX IF NOT EXISTS idx_file_access_user_id ON storage.file_access(user_id);
CREATE INDEX IF NOT EXISTS idx_file_shards_file_id ON storage.file_shards(file_id);

CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit.audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON audit.audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_audit_logs_service ON audit.audit_logs(service);
CREATE INDEX IF NOT EXISTS idx_security_events_user_id ON audit.security_events(user_id);
CREATE INDEX IF NOT EXISTS idx_security_events_timestamp ON audit.security_events(timestamp);
CREATE INDEX IF NOT EXISTS idx_security_events_type ON audit.security_events(event_type);
CREATE INDEX IF NOT EXISTS idx_access_attempts_file_id ON audit.access_attempts(file_id);
CREATE INDEX IF NOT EXISTS idx_access_attempts_timestamp ON audit.access_attempts(timestamp);

-- Insert default permissions
INSERT INTO access_control.permissions (name, description) VALUES
('file:read', 'Read file content'),
('file:write', 'Write/modify file content'),
('file:delete', 'Delete file'),
('file:share', 'Share file with other users'),
('user:read', 'Read user information'),
('user:write', 'Modify user information'),
('admin:all', 'Full administrative access')
ON CONFLICT (name) DO NOTHING;

-- Insert default role permissions
INSERT INTO access_control.role_permissions (role_name, permission_id) 
SELECT 'User', id FROM access_control.permissions WHERE name IN ('file:read', 'file:write', 'file:share')
ON CONFLICT DO NOTHING;

INSERT INTO access_control.role_permissions (role_name, permission_id) 
SELECT 'Admin', id FROM access_control.permissions
ON CONFLICT DO NOTHING;

-- Create a default admin user (password: Admin123!)
INSERT INTO identity.users (email, password_hash, first_name, last_name, roles, requires_mfa)
VALUES (
    'admin@hardlock.com',
    '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/HS.iK8O', -- Admin123!
    'System',
    'Administrator',
    '["Admin"]',
    true
) ON CONFLICT (email) DO NOTHING; 