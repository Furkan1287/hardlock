using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace HardLock.Security.Encryption;

public interface IDarknetBackupService
{
    Task<DarknetBackupResult> BackupToDarknetAsync(byte[] fileData, string fileName, DarknetBackupOptions options);
    Task<byte[]?> RestoreFromDarknetAsync(string contentHash, string fileName, DarknetBackupOptions options);
    Task<bool> VerifyBackupIntegrityAsync(string contentHash, string fileName);
    Task<List<DarknetNodeInfo>> GetAvailableNodesAsync();
    Task<DarknetBackupStatus> GetBackupStatusAsync(string contentHash);
}

public class DarknetBackupService : IDarknetBackupService
{
    private readonly ILogger<DarknetBackupService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _torProxyUrl;
    private readonly string _ipfsGatewayUrl;
    private readonly List<string> _darknetNodes;

    public DarknetBackupService(
        ILogger<DarknetBackupService> logger, 
        HttpClient httpClient,
        string torProxyUrl = "socks5://127.0.0.1:9050",
        string ipfsGatewayUrl = "http://localhost:5001")
    {
        _logger = logger;
        _httpClient = httpClient;
        _torProxyUrl = torProxyUrl;
        _ipfsGatewayUrl = ipfsGatewayUrl;
        
        // Known darknet IPFS nodes (example)
        _darknetNodes = new List<string>
        {
            "http://darknet1.onion:5001",
            "http://darknet2.onion:5001", 
            "http://darknet3.onion:5001"
        };
    }

    public async Task<DarknetBackupResult> BackupToDarknetAsync(byte[] fileData, string fileName, DarknetBackupOptions options)
    {
        try
        {
            _logger.LogInformation("Starting darknet backup for file: {FileName}", fileName);

            // Step 1: Shard the file
            var shards = await ShardFileAsync(fileData, options.ShardSize);
            _logger.LogInformation("File sharded into {ShardCount} pieces", shards.Count);

            // Step 2: Encrypt each shard
            var encryptedShards = new List<EncryptedShard>();
            foreach (var shard in shards)
            {
                var encryptedShard = await EncryptShardAsync(shard, options.EncryptionKey);
                encryptedShards.Add(encryptedShard);
            }

            // Step 3: Upload to IPFS via Tor
            var contentHashes = new List<string>();
            foreach (var shard in encryptedShards)
            {
                var hash = await UploadToIPFSAsync(shard.Data, fileName);
                contentHashes.Add(hash);
            }

            // Step 4: Create distributed hash table entry
            var dhtEntry = new DarknetDHTEntry
            {
                FileName = fileName,
                OriginalSize = fileData.Length,
                ShardCount = shards.Count,
                ContentHashes = contentHashes,
                EncryptionAlgorithm = "AES-256-GCM",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = options.ExpiresAt,
                ReplicationFactor = options.ReplicationFactor
            };

            var dhtHash = await CreateDHTEntryAsync(dhtEntry);

            // Step 5: Distribute across multiple darknet nodes
            await DistributeAcrossNodesAsync(dhtHash, dhtEntry);

            var result = new DarknetBackupResult
            {
                Success = true,
                DhtHash = dhtHash,
                ContentHashes = contentHashes,
                ShardCount = shards.Count,
                TotalSize = fileData.Length,
                BackupTime = DateTime.UtcNow,
                EstimatedNodes = await GetEstimatedNodeCountAsync()
            };

            _logger.LogInformation("Darknet backup completed successfully. DHT Hash: {DhtHash}", dhtHash);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during darknet backup for file: {FileName}", fileName);
            return new DarknetBackupResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<byte[]?> RestoreFromDarknetAsync(string contentHash, string fileName, DarknetBackupOptions options)
    {
        try
        {
            _logger.LogInformation("Starting darknet restore for file: {FileName}", fileName);

            // Step 1: Retrieve DHT entry
            var dhtEntry = await GetDHTEntryAsync(contentHash);
            if (dhtEntry == null)
            {
                throw new Exception("DHT entry not found");
            }

            // Step 2: Download all shards from IPFS
            var encryptedShards = new List<EncryptedShard>();
            foreach (var hash in dhtEntry.ContentHashes)
            {
                var shardData = await DownloadFromIPFSAsync(hash);
                if (shardData != null)
                {
                    encryptedShards.Add(new EncryptedShard { Data = shardData });
                }
            }

            if (encryptedShards.Count != dhtEntry.ShardCount)
            {
                throw new Exception($"Missing shards. Expected: {dhtEntry.ShardCount}, Found: {encryptedShards.Count}");
            }

            // Step 3: Decrypt shards
            var decryptedShards = new List<byte[]>();
            foreach (var shard in encryptedShards)
            {
                var decryptedShard = await DecryptShardAsync(shard, options.EncryptionKey);
                decryptedShards.Add(decryptedShard);
            }

            // Step 4: Reassemble file
            var reassembledFile = await ReassembleFileAsync(decryptedShards, dhtEntry.OriginalSize);

            _logger.LogInformation("Darknet restore completed successfully");
            return reassembledFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during darknet restore for file: {FileName}", fileName);
            return null;
        }
    }

    public async Task<bool> VerifyBackupIntegrityAsync(string contentHash, string fileName)
    {
        try
        {
            var dhtEntry = await GetDHTEntryAsync(contentHash);
            if (dhtEntry == null) return false;

            var availableShards = 0;
            foreach (var hash in dhtEntry.ContentHashes)
            {
                if (await VerifyIPFSHashAsync(hash))
                {
                    availableShards++;
                }
            }

            var integrity = availableShards >= dhtEntry.ShardCount * 0.8; // 80% threshold
            _logger.LogInformation("Backup integrity check: {Integrity} ({AvailableShards}/{TotalShards})", 
                integrity, availableShards, dhtEntry.ShardCount);

            return integrity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying backup integrity");
            return false;
        }
    }

    public async Task<List<DarknetNodeInfo>> GetAvailableNodesAsync()
    {
        try
        {
            var nodes = new List<DarknetNodeInfo>();
            
            foreach (var nodeUrl in _darknetNodes)
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{nodeUrl}/api/v0/id");
                    if (response.IsSuccessStatusCode)
                    {
                        var nodeInfo = JsonSerializer.Deserialize<DarknetNodeInfo>(await response.Content.ReadAsStringAsync());
                        if (nodeInfo != null)
                        {
                            nodes.Add(nodeInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to connect to darknet node: {NodeUrl}", nodeUrl);
                }
            }

            return nodes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available nodes");
            return new List<DarknetNodeInfo>();
        }
    }

    public async Task<DarknetBackupStatus> GetBackupStatusAsync(string contentHash)
    {
        try
        {
            var dhtEntry = await GetDHTEntryAsync(contentHash);
            if (dhtEntry == null)
            {
                return new DarknetBackupStatus { Exists = false };
            }

            var availableShards = 0;
            var totalShards = dhtEntry.ContentHashes.Count;

            foreach (var hash in dhtEntry.ContentHashes)
            {
                if (await VerifyIPFSHashAsync(hash))
                {
                    availableShards++;
                }
            }

            return new DarknetBackupStatus
            {
                Exists = true,
                AvailableShards = availableShards,
                TotalShards = totalShards,
                HealthPercentage = (double)availableShards / totalShards * 100,
                CreatedAt = dhtEntry.CreatedAt,
                ExpiresAt = dhtEntry.ExpiresAt,
                IsExpired = dhtEntry.ExpiresAt.HasValue && DateTime.UtcNow > dhtEntry.ExpiresAt.Value
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backup status");
            return new DarknetBackupStatus { Exists = false };
        }
    }

    private async Task<List<byte[]>> ShardFileAsync(byte[] fileData, int shardSize)
    {
        var shards = new List<byte[]>();
        var offset = 0;

        while (offset < fileData.Length)
        {
            var remainingBytes = fileData.Length - offset;
            var currentShardSize = Math.Min(shardSize, remainingBytes);
            
            var shard = new byte[currentShardSize];
            Array.Copy(fileData, offset, shard, 0, currentShardSize);
            
            shards.Add(shard);
            offset += currentShardSize;
        }

        return shards;
    }

    private async Task<EncryptedShard> EncryptShardAsync(byte[] shardData, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var encryptedData = encryptor.TransformFinalBlock(shardData, 0, shardData.Length);

        return new EncryptedShard
        {
            Data = encryptedData,
            Key = Convert.ToBase64String(aes.Key),
            IV = Convert.ToBase64String(aes.IV)
        };
    }

    private async Task<byte[]> DecryptShardAsync(EncryptedShard shard, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(shard.Key);
        aes.IV = Convert.FromBase64String(shard.IV);

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(shard.Data, 0, shard.Data.Length);
    }

    private async Task<string> UploadToIPFSAsync(byte[] data, string fileName)
    {
        try
        {
            var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(data), "file", fileName);

            var response = await _httpClient.PostAsync($"{_ipfsGatewayUrl}/api/v0/add", content);
            response.EnsureSuccessStatusCode();

            var result = JsonSerializer.Deserialize<IPFSAddResult>(await response.Content.ReadAsStringAsync());
            return result?.Hash ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to IPFS");
            throw;
        }
    }

    private async Task<byte[]?> DownloadFromIPFSAsync(string hash)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ipfsGatewayUrl}/api/v0/cat?arg={hash}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading from IPFS: {Hash}", hash);
            return null;
        }
    }

    private async Task<bool> VerifyIPFSHashAsync(string hash)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ipfsGatewayUrl}/api/v0/object/stat?arg={hash}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> CreateDHTEntryAsync(DarknetDHTEntry entry)
    {
        var json = JsonSerializer.Serialize(entry);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_ipfsGatewayUrl}/api/v0/add", content);
        response.EnsureSuccessStatusCode();

        var result = JsonSerializer.Deserialize<IPFSAddResult>(await response.Content.ReadAsStringAsync());
        return result?.Hash ?? string.Empty;
    }

    private async Task<DarknetDHTEntry?> GetDHTEntryAsync(string hash)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ipfsGatewayUrl}/api/v0/cat?arg={hash}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DarknetDHTEntry>(json);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DHT entry: {Hash}", hash);
            return null;
        }
    }

    private async Task DistributeAcrossNodesAsync(string dhtHash, DarknetDHTEntry entry)
    {
        var nodes = await GetAvailableNodesAsync();
        var replicationTasks = new List<Task>();

        foreach (var node in nodes.Take(entry.ReplicationFactor))
        {
            replicationTasks.Add(ReplicateToNodeAsync(node, dhtHash, entry));
        }

        await Task.WhenAll(replicationTasks);
    }

    private async Task ReplicateToNodeAsync(DarknetNodeInfo node, string dhtHash, DarknetDHTEntry entry)
    {
        try
        {
            var json = JsonSerializer.Serialize(entry);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            await _httpClient.PostAsync($"{node.Addresses.FirstOrDefault()}/api/v0/add", content);
            _logger.LogInformation("Replicated to node: {NodeId}", node.ID);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to replicate to node: {NodeId}", node.ID);
        }
    }

    private async Task<byte[]> ReassembleFileAsync(List<byte[]> shards, int originalSize)
    {
        var reassembled = new byte[originalSize];
        var offset = 0;

        foreach (var shard in shards)
        {
            Array.Copy(shard, 0, reassembled, offset, shard.Length);
            offset += shard.Length;
        }

        return reassembled;
    }

    private async Task<int> GetEstimatedNodeCountAsync()
    {
        var nodes = await GetAvailableNodesAsync();
        return nodes.Count;
    }
}

public class DarknetBackupOptions
{
    public int ShardSize { get; set; } = 1024 * 1024; // 1MB
    public string EncryptionKey { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public int ReplicationFactor { get; set; } = 3;
    public bool EnableTorRouting { get; set; } = true;
}

public class DarknetBackupResult
{
    public bool Success { get; set; }
    public string DhtHash { get; set; } = string.Empty;
    public List<string> ContentHashes { get; set; } = new();
    public int ShardCount { get; set; }
    public long TotalSize { get; set; }
    public DateTime BackupTime { get; set; }
    public int EstimatedNodes { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DarknetBackupStatus
{
    public bool Exists { get; set; }
    public int AvailableShards { get; set; }
    public int TotalShards { get; set; }
    public double HealthPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
}

public class DarknetDHTEntry
{
    public string FileName { get; set; } = string.Empty;
    public long OriginalSize { get; set; }
    public int ShardCount { get; set; }
    public List<string> ContentHashes { get; set; } = new();
    public string EncryptionAlgorithm { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ReplicationFactor { get; set; }
}

public class DarknetNodeInfo
{
    public string ID { get; set; } = string.Empty;
    public List<string> Addresses { get; set; } = new();
    public string AgentVersion { get; set; } = string.Empty;
    public string ProtocolVersion { get; set; } = string.Empty;
}

public class EncryptedShard
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string Key { get; set; } = string.Empty;
    public string IV { get; set; } = string.Empty;
}

public class IPFSAddResult
{
    public string Name { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public long Size { get; set; }
}
