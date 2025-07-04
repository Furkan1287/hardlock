using HardLock.Storage.Models;
using HardLock.Storage.Data;
using HardLock.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HardLock.Storage.Services;

public class StorageService : IStorageService
{
    private readonly StorageDbContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger<StorageService> _logger;
    private readonly string _storageBasePath;

    public StorageService(StorageDbContext context, Microsoft.Extensions.Logging.ILogger<StorageService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _storageBasePath = configuration["Storage:BasePath"] ?? "storage";
        
        // Ensure storage directory exists
        Directory.CreateDirectory(_storageBasePath);
    }

    public async Task<FileStorageResponse> StoreFileAsync(FileStorageRequest request)
    {
        try
        {
            var fileId = Guid.NewGuid();
            var fileName = $"{fileId}_{request.FileName}";
            var filePath = Path.Combine(_storageBasePath, fileName);

            // Write file to disk
            await System.IO.File.WriteAllBytesAsync(filePath, request.FileData);

            // Create file record
            var file = new HardLock.Shared.Models.File
            {
                Id = fileId,
                FileName = request.FileName,
                FilePath = filePath,
                ContentType = request.ContentType,
                FileSize = request.FileSize,
                IsEncrypted = request.IsEncrypted,
                StorageTier = request.StorageTier ?? "standard",
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Files.Add(file);

            // Add metadata if provided
            if (request.Metadata != null)
            {
                foreach (var kvp in request.Metadata)
                {
                    var metadata = new FileMetadata
                    {
                        Id = Guid.NewGuid(),
                        FileId = fileId,
                        Key = kvp.Key,
                        Value = kvp.Value
                    };
                    _context.FileMetadata.Add(metadata);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("File stored successfully: {FileId} - {FileName}", fileId, request.FileName);

            return new FileStorageResponse
            {
                FileId = fileId.ToString(),
                FileName = request.FileName,
                FileUrl = $"/api/storage/files/{fileId}",
                FileSize = request.FileSize,
                ContentType = request.ContentType,
                UploadedAt = file.CreatedAt,
                StorageLocation = filePath,
                IsEncrypted = request.IsEncrypted,
                Metadata = request.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing file: {FileName}", request.FileName);
            throw;
        }
    }

    public async Task<FileRetrievalResponse> RetrieveFileAsync(FileRetrievalRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.FileId, out var fileId))
            {
                throw new ArgumentException("Invalid file ID format");
            }

            var file = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == fileId && (request.UserId == null || f.UserId == request.UserId));

            if (file == null)
            {
                throw new FileNotFoundException($"File not found: {request.FileId}");
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                throw new FileNotFoundException($"Physical file not found at path: {file.FilePath}");
            }

            var fileData = await System.IO.File.ReadAllBytesAsync(file.FilePath);

            _logger.LogInformation("File retrieved successfully: {FileId} - {FileName}", fileId, file.FileName);

            return new FileRetrievalResponse
            {
                FileId = file.Id.ToString(),
                FileName = file.FileName,
                FileData = fileData,
                ContentType = file.ContentType,
                FileSize = file.FileSize,
                RetrievedAt = DateTime.UtcNow,
                IsEncrypted = file.IsEncrypted
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file: {FileId}", request.FileId);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(FileDeleteRequest request)
    {
        try
        {
            if (!Guid.TryParse(request.FileId, out var fileId))
            {
                return false;
            }

            var file = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == fileId && (request.UserId == null || f.UserId == request.UserId));

            if (file == null)
            {
                return false;
            }

            // Delete physical file
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }

            // Delete from database
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File deleted successfully: {FileId} - {FileName}", fileId, file.FileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileId}", request.FileId);
            return false;
        }
    }

    public async Task<FileListResponse> ListFilesAsync(FileListRequest request)
    {
        try
        {
            var query = _context.Files.AsQueryable();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(f => f.UserId == request.UserId);
            }

            if (!string.IsNullOrEmpty(request.FileType))
            {
                query = query.Where(f => f.ContentType.StartsWith(request.FileType));
            }

            if (request.FromDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(f => f.CreatedAt <= request.ToDate.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var files = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new FileStorageResponse
                {
                    FileId = f.Id.ToString(),
                    FileName = f.FileName,
                    FileUrl = $"/api/storage/files/{f.Id}",
                    FileSize = f.FileSize,
                    ContentType = f.ContentType,
                    UploadedAt = f.CreatedAt,
                    StorageLocation = f.FilePath,
                    IsEncrypted = f.IsEncrypted
                })
                .ToListAsync();

            return new FileListResponse
            {
                Files = files,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files");
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileId, string? userId = null)
    {
        try
        {
            if (!Guid.TryParse(fileId, out var id))
            {
                return false;
            }

            var query = _context.Files.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(f => f.UserId == userId);
            }

            return await query.AnyAsync(f => f.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FileId}", fileId);
            return false;
        }
    }

    public async Task<long> GetStorageUsageAsync(string? userId = null)
    {
        try
        {
            var query = _context.Files.AsQueryable();
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(f => f.UserId == userId);
            }

            return await query.SumAsync(f => f.FileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating storage usage");
            return 0;
        }
    }

    public async Task<bool> MoveToColdStorageAsync(string fileId, string? userId = null)
    {
        try
        {
            if (!Guid.TryParse(fileId, out var id))
            {
                return false;
            }

            var file = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == id && (userId == null || f.UserId == userId));

            if (file == null)
            {
                return false;
            }

            file.StorageTier = "cold";
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("File moved to cold storage: {FileId}", fileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file to cold storage: {FileId}", fileId);
            return false;
        }
    }

    public async Task<bool> RestoreFromColdStorageAsync(string fileId, string? userId = null)
    {
        try
        {
            if (!Guid.TryParse(fileId, out var id))
            {
                return false;
            }

            var file = await _context.Files
                .FirstOrDefaultAsync(f => f.Id == id && (userId == null || f.UserId == userId));

            if (file == null)
            {
                return false;
            }

            file.StorageTier = "standard";
            file.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("File restored from cold storage: {FileId}", fileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring file from cold storage: {FileId}", fileId);
            return false;
        }
    }
} 