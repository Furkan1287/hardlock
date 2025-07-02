using HardLock.Encryption.Models;

namespace HardLock.Encryption.Services;

public interface IEncryptionService
{
    Task<EncryptionResponse> EncryptFileAsync(FileEncryptionRequest request);
    Task<DecryptionResponse> DecryptFileAsync(FileDecryptionRequest request);
    Task<ShardEncryptionResponse> ShardEncryptAsync(ShardEncryptionRequest request);
    Task<DecryptionResponse> ShardDecryptAsync(ShardDecryptionRequest request);
    Task<bool> ValidatePasswordAsync(PasswordValidationRequest request);
    
    // Timelock Methods
    Task<TimelockEncryptionResponse> EncryptWithTimelockAsync(TimelockEncryptionRequest request);
    Task<DecryptionResponse> DecryptWithTimelockAsync(TimelockDecryptionRequest request);
} 