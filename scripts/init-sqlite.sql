-- HARDLOCK SQLite Database Initialization Script

-- Create tables
CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    email TEXT UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    first_name TEXT,
    last_name TEXT,
    is_active INTEGER DEFAULT 1,
    requires_mfa INTEGER DEFAULT 0,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    last_login_at TEXT,
    roles TEXT DEFAULT '["User"]',
    metadata TEXT DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS refresh_tokens (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    token TEXT UNIQUE NOT NULL,
    expires_at TEXT NOT NULL,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    is_revoked INTEGER DEFAULT 0,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_active ON users(is_active);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires ON refresh_tokens(expires_at);

-- Insert default admin user
-- Password: Admin123! (BCrypt hash)
INSERT OR IGNORE INTO users (email, password_hash, first_name, last_name, roles, is_active)
VALUES (
    'admin@hardlock.com',
    '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',
    'Admin',
    'User',
    '["Admin", "User"]',
    1
);

-- Create files table for storage service
CREATE TABLE IF NOT EXISTS files (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    owner_id INTEGER NOT NULL,
    file_name TEXT NOT NULL,
    original_file_name TEXT,
    content_type TEXT DEFAULT 'application/octet-stream',
    file_size INTEGER NOT NULL,
    file_hash TEXT NOT NULL,
    encryption_hash TEXT NOT NULL,
    integrity_hash TEXT,
    hash_algorithm TEXT DEFAULT 'SHA256',
    integrity_verified INTEGER DEFAULT 0,
    last_integrity_check TEXT,
    integrity_check_count INTEGER DEFAULT 0,
    hash_verification_enabled INTEGER DEFAULT 1,
    is_encrypted INTEGER DEFAULT 1,
    is_sharded INTEGER DEFAULT 0,
    shard_count INTEGER DEFAULT 1,
    expires_at TEXT,
    unlock_at TEXT,
    geo_fencing_enabled INTEGER DEFAULT 0,
    allowed_regions TEXT,
    allowed_countries TEXT,
    allowed_cities TEXT,
    allowed_location_lat REAL,
    allowed_location_lng REAL,
    allowed_radius REAL,
    has_timelock INTEGER DEFAULT 0,
    timelock_block_number INTEGER,
    timelock_encrypted_key TEXT,
    timelock_public_key TEXT,
    timelock_type TEXT DEFAULT 'none',
    darknet_backup_enabled INTEGER DEFAULT 0,
    darknet_dht_hash TEXT,
    darknet_content_hashes TEXT,
    self_destruct_enabled INTEGER DEFAULT 1,
    max_access_attempts INTEGER DEFAULT 3,
    current_access_attempts INTEGER DEFAULT 0,
    status TEXT DEFAULT 'Active',
    access_level TEXT DEFAULT 'Private',
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    last_accessed_at TEXT,
    last_modified_at TEXT DEFAULT CURRENT_TIMESTAMP,
    metadata TEXT DEFAULT '{}',
    FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS file_access (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    file_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    permission TEXT NOT NULL,
    granted_at TEXT DEFAULT CURRENT_TIMESTAMP,
    expires_at TEXT,
    is_active INTEGER DEFAULT 1,
    FOREIGN KEY (file_id) REFERENCES files(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    UNIQUE(file_id, user_id)
);

CREATE TABLE IF NOT EXISTS file_shards (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    file_id INTEGER NOT NULL,
    shard_index INTEGER NOT NULL,
    shard_hash TEXT NOT NULL,
    encrypted_data TEXT NOT NULL,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (file_id) REFERENCES files(id) ON DELETE CASCADE,
    UNIQUE(file_id, shard_index)
);

-- Create audit tables
CREATE TABLE IF NOT EXISTS audit_logs (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER,
    user_email TEXT,
    service TEXT NOT NULL,
    action TEXT NOT NULL,
    resource_type TEXT,
    resource_id TEXT,
    severity TEXT DEFAULT 'Info',
    status TEXT DEFAULT 'Success',
    ip_address TEXT,
    user_agent TEXT,
    location_lat REAL,
    location_lng REAL,
    request_data TEXT,
    response_data TEXT,
    error_message TEXT,
    metadata TEXT DEFAULT '{}',
    timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    duration_ms INTEGER
);

CREATE TABLE IF NOT EXISTS security_events (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER,
    user_email TEXT,
    event_type TEXT NOT NULL,
    severity TEXT DEFAULT 'Medium',
    file_id INTEGER,
    ip_address TEXT,
    user_agent TEXT,
    location_lat REAL,
    location_lng REAL,
    description TEXT NOT NULL,
    details TEXT DEFAULT '{}',
    requires_action INTEGER DEFAULT 0,
    recommended_action TEXT,
    timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    is_resolved INTEGER DEFAULT 0,
    resolved_at TEXT,
    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (file_id) REFERENCES files(id)
);

CREATE TABLE IF NOT EXISTS access_attempts (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    file_id INTEGER NOT NULL,
    user_id INTEGER,
    user_email TEXT,
    is_successful INTEGER NOT NULL,
    failure_reason TEXT,
    ip_address TEXT,
    user_agent TEXT,
    location_lat REAL,
    location_lng REAL,
    timestamp TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (file_id) REFERENCES files(id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users(id)
);

-- Create access control tables
CREATE TABLE IF NOT EXISTS permissions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT UNIQUE NOT NULL,
    description TEXT,
    created_at TEXT DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS role_permissions (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    role_name TEXT NOT NULL,
    permission_id INTEGER NOT NULL,
    granted_at TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE,
    UNIQUE(role_name, permission_id)
);

CREATE TABLE IF NOT EXISTS user_roles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    role_name TEXT NOT NULL,
    granted_at TEXT DEFAULT CURRENT_TIMESTAMP,
    granted_by INTEGER,
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    FOREIGN KEY (granted_by) REFERENCES users(id),
    UNIQUE(user_id, role_name)
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_files_owner_id ON files(owner_id);
CREATE INDEX IF NOT EXISTS idx_files_hash ON files(file_hash);
CREATE INDEX IF NOT EXISTS idx_files_status ON files(status);
CREATE INDEX IF NOT EXISTS idx_files_expires ON files(expires_at);
CREATE INDEX IF NOT EXISTS idx_files_geo_fencing_enabled ON files(geo_fencing_enabled);
CREATE INDEX IF NOT EXISTS idx_files_timelock ON files(has_timelock);
CREATE INDEX IF NOT EXISTS idx_files_darknet_backup ON files(darknet_backup_enabled);
CREATE INDEX IF NOT EXISTS idx_file_access_file_id ON file_access(file_id);
CREATE INDEX IF NOT EXISTS idx_file_access_user_id ON file_access(user_id);
CREATE INDEX IF NOT EXISTS idx_file_shards_file_id ON file_shards(file_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_user_id ON audit_logs(user_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_timestamp ON audit_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_audit_logs_service ON audit_logs(service);
CREATE INDEX IF NOT EXISTS idx_security_events_user_id ON security_events(user_id);
CREATE INDEX IF NOT EXISTS idx_security_events_timestamp ON security_events(timestamp);
CREATE INDEX IF NOT EXISTS idx_security_events_type ON security_events(event_type);
CREATE INDEX IF NOT EXISTS idx_access_attempts_file_id ON access_attempts(file_id);
CREATE INDEX IF NOT EXISTS idx_access_attempts_timestamp ON access_attempts(timestamp);

-- Insert default permissions
INSERT OR IGNORE INTO permissions (name, description) VALUES
('files.read', 'Read files'),
('files.write', 'Write files'),
('files.delete', 'Delete files'),
('files.share', 'Share files'),
('admin.users', 'Manage users'),
('admin.system', 'System administration');

-- Insert default role permissions
INSERT OR IGNORE INTO role_permissions (role_name, permission_id) VALUES
('User', 1), -- files.read
('User', 2), -- files.write
('User', 4), -- files.share
('Admin', 1), -- files.read
('Admin', 2), -- files.write
('Admin', 3), -- files.delete
('Admin', 4), -- files.share
('Admin', 5), -- admin.users
('Admin', 6); -- admin.system

-- Insert default user roles
INSERT OR IGNORE INTO user_roles (user_id, role_name) VALUES
(1, 'Admin'),
(1, 'User'); 