using HardLock.Storage.Models;

namespace HardLock.Storage.Services;

public interface IStorageService
{
    Task<FileStorageResponse> StoreFileAsync(FileStorageRequest request);
    Task<FileRetrievalResponse> RetrieveFileAsync(FileRetrievalRequest request);
    Task<bool> DeleteFileAsync(FileDeleteRequest request);
    Task<FileListResponse> ListFilesAsync(FileListRequest request);
    Task<bool> FileExistsAsync(string fileId, string? userId = null);
    Task<long> GetStorageUsageAsync(string? userId = null);
    Task<bool> MoveToColdStorageAsync(string fileId, string? userId = null);
    Task<bool> RestoreFromColdStorageAsync(string fileId, string? userId = null);
} 