using HardLock.Identity.Data;
using HardLock.Identity.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Dapper;

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

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var query = "SELECT Id, FirstName, LastName, Email, IsActive, CreatedAt, LastLoginAt FROM Users WHERE Email = @Email";
            
            return await connection.QueryFirstOrDefaultAsync<UserResponse>(query, new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var query = "SELECT Id, FirstName, LastName, Email, IsActive, CreatedAt, LastLoginAt FROM Users WHERE Id = @Id";
            
            return await connection.QueryFirstOrDefaultAsync<UserResponse>(query, new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id: {Id}", id);
            return null;
        }
    }

    public async Task<UserResponse> CreateUserAsync(UserRegistrationRequest request, string hashedPassword)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var query = @"
                INSERT INTO Users (Id, FirstName, LastName, Email, PasswordHash, IsActive, CreatedAt)
                VALUES (@Id, @FirstName, @LastName, @Email, @PasswordHash, @IsActive, @CreatedAt)";
            
            var user = new UserResponse
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await connection.ExecuteAsync(query, new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                PasswordHash = hashedPassword,
                user.IsActive,
                user.CreatedAt
            });
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", request.Email);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(Guid id, UserUpdateRequest request)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var query = @"
                UPDATE Users 
                SET FirstName = COALESCE(@FirstName, FirstName),
                    LastName = COALESCE(@LastName, LastName),
                    Email = COALESCE(@Email, Email),
                    IsActive = COALESCE(@IsActive, IsActive)
                WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(query, new
            {
                Id = id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.IsActive
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(Guid id)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var query = "UPDATE Users SET LastLoginAt = @LastLoginAt WHERE Id = @Id";
            
            var rowsAffected = await connection.ExecuteAsync(query, new
            {
                Id = id,
                LastLoginAt = DateTime.UtcNow
            });
            
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {Id}", id);
            return false;
        }
    }

    public async Task<string?> GetPasswordHashAsync(string email)
    {
        try
        {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var query = "SELECT PasswordHash FROM Users WHERE Email = @Email";
            
            return await connection.QueryFirstOrDefaultAsync<string>(query, new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting password hash for user: {Email}", email);
            return null;
        }
    }
} 