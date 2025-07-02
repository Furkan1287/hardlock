using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text.Json;
using HardLock.Identity.Data;
using HardLock.Shared.Models;

namespace HardLock.Identity.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbConnectionFactory connectionFactory, ILogger<UserRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = @"
                SELECT id, email, password_hash, first_name, last_name, is_active, 
                       requires_mfa, created_at, last_login_at, roles, metadata
                FROM identity.users 
                WHERE id = @Id";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
            
            if (user != null)
            {
                // Parse JSON fields
                user.Roles = ParseJsonArray<string>(user.Roles?.ToString() ?? "[]");
                user.Metadata = ParseJsonObject(user.Metadata?.ToString() ?? "{}");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = @"
                SELECT id, email, password_hash, first_name, last_name, is_active, 
                       requires_mfa, created_at, last_login_at, roles, metadata
                FROM identity.users 
                WHERE email = @Email";

            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email.ToLowerInvariant() });
            
            if (user != null)
            {
                // Parse JSON fields
                user.Roles = ParseJsonArray<string>(user.Roles?.ToString() ?? "[]");
                user.Metadata = ParseJsonObject(user.Metadata?.ToString() ?? "{}");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = @"
                SELECT id, email, password_hash, first_name, last_name, is_active, 
                       requires_mfa, created_at, last_login_at, roles, metadata
                FROM identity.users 
                ORDER BY created_at DESC";

            var users = await connection.QueryAsync<User>(sql);
            
            foreach (var user in users)
            {
                user.Roles = ParseJsonArray<string>(user.Roles?.ToString() ?? "[]");
                user.Metadata = ParseJsonObject(user.Metadata?.ToString() ?? "{}");
            }

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }
    }

    public async Task<User> CreateAsync(User user)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = @"
                INSERT INTO identity.users (id, email, password_hash, first_name, last_name, 
                                          is_active, requires_mfa, created_at, roles, metadata)
                VALUES (@Id, @Email, @PasswordHash, @FirstName, @LastName, 
                       @IsActive, @RequiresMfa, @CreatedAt, @Roles, @Metadata)
                RETURNING id, email, password_hash, first_name, last_name, is_active, 
                         requires_mfa, created_at, last_login_at, roles, metadata";

            var parameters = new
            {
                user.Id,
                Email = user.Email.ToLowerInvariant(),
                user.PasswordHash,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.RequiresMfa,
                user.CreatedAt,
                Roles = JsonSerializer.Serialize(user.Roles),
                Metadata = JsonSerializer.Serialize(user.Metadata)
            };

            var createdUser = await connection.QueryFirstAsync<User>(sql, parameters);
            
            createdUser.Roles = user.Roles;
            createdUser.Metadata = user.Metadata;

            _logger.LogInformation("User created successfully: {UserId}", user.Id);
            return createdUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = @"
                UPDATE identity.users 
                SET first_name = @FirstName, last_name = @LastName, is_active = @IsActive,
                    requires_mfa = @RequiresMfa, last_modified_at = @LastModifiedAt,
                    roles = @Roles, metadata = @Metadata
                WHERE id = @Id
                RETURNING id, email, password_hash, first_name, last_name, is_active, 
                         requires_mfa, created_at, last_login_at, roles, metadata";

            var parameters = new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.RequiresMfa,
                LastModifiedAt = DateTime.UtcNow,
                Roles = JsonSerializer.Serialize(user.Roles),
                Metadata = JsonSerializer.Serialize(user.Metadata)
            };

            var updatedUser = await connection.QueryFirstAsync<User>(sql, parameters);
            
            updatedUser.Roles = user.Roles;
            updatedUser.Metadata = user.Metadata;

            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            return updatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = "DELETE FROM identity.users WHERE id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            
            _logger.LogInformation("User deleted: {UserId}, Rows affected: {RowsAffected}", id, rowsAffected);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string email)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = "SELECT COUNT(1) FROM identity.users WHERE email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email.ToLowerInvariant() });
            
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists: {Email}", email);
            throw;
        }
    }

    public async Task<bool> ValidatePasswordAsync(string email, string passwordHash)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = "SELECT password_hash FROM identity.users WHERE email = @Email";
            var storedHash = await connection.ExecuteScalarAsync<string>(sql, new { Email = email.ToLowerInvariant() });
            
            return storedHash == passwordHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user: {Email}", email);
            return false;
        }
    }

    public async Task UpdateLastLoginAsync(Guid id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            
            const string sql = "UPDATE identity.users SET last_login_at = @LastLoginAt WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id, LastLoginAt = DateTime.UtcNow });
            
            _logger.LogInformation("Last login updated for user: {UserId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", id);
            throw;
        }
    }

    private static List<T> ParseJsonArray<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    private static Dictionary<string, string> ParseJsonObject(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
} 