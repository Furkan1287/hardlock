using FluentValidation;
using HardLock.Encryption.Models;

namespace HardLock.Encryption.Validators;

public class FileEncryptionRequestValidator : AbstractValidator<FileEncryptionRequest>
{
    public FileEncryptionRequestValidator()
    {
        RuleFor(x => x.FileData)
            .NotEmpty()
            .WithMessage("File data is required")
            .Must(data => data.Length > 0)
            .WithMessage("File data cannot be empty")
            .Must(data => data.Length <= 100 * 1024 * 1024) // 100MB limit
            .WithMessage("File size cannot exceed 100MB");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .WithMessage("Password must be between 8 and 128 characters");

        RuleFor(x => x.KeyDerivationIterations)
            .InclusiveBetween(10000, 1000000)
            .WithMessage("Key derivation iterations must be between 10,000 and 1,000,000");

        RuleFor(x => x.ShardSize)
            .InclusiveBetween(1024, 1024 * 1024) // 1KB to 1MB
            .When(x => x.EnableSharding)
            .WithMessage("Shard size must be between 1KB and 1MB when sharding is enabled");

        RuleFor(x => x.FileName)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.FileName))
            .WithMessage("File name cannot exceed 255 characters");
    }
} 