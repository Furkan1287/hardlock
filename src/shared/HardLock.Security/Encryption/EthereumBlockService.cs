using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HardLock.Security.Encryption;

public interface IEthereumBlockService
{
    Task<long> GetCurrentBlockNumberAsync();
    Task<DateTime> GetBlockTimestampAsync(long blockNumber);
    Task<bool> IsBlockReachedAsync(long targetBlock);
    Task<EthereumBlockInfo> GetBlockInfoAsync(long blockNumber);
}

public class EthereumBlockService : IEthereumBlockService
{
    private readonly ILogger<EthereumBlockService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _ethereumRpcUrl;

    public EthereumBlockService(ILogger<EthereumBlockService> logger, HttpClient httpClient, string ethereumRpcUrl = "https://mainnet.infura.io/v3/YOUR-PROJECT-ID")
    {
        _logger = logger;
        _httpClient = httpClient;
        _ethereumRpcUrl = ethereumRpcUrl;
    }

    public async Task<long> GetCurrentBlockNumberAsync()
    {
        try
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_blockNumber",
                @params = new object[] { },
                id = 1
            };

            var response = await SendRpcRequestAsync(request);
            var hexBlockNumber = response.GetProperty("result").GetString();
            
            // Convert hex to decimal
            var blockNumber = Convert.ToInt64(hexBlockNumber, 16);
            
            _logger.LogDebug("Current Ethereum block number: {BlockNumber}", blockNumber);
            return blockNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current block number");
            // Fallback: return a reasonable estimate based on current time
            // Ethereum produces ~1 block every 12 seconds
            var estimatedBlock = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1438269988) / 12;
            return Math.Max(0, estimatedBlock);
        }
    }

    public async Task<DateTime> GetBlockTimestampAsync(long blockNumber)
    {
        try
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_getBlockByNumber",
                @params = new object[] { $"0x{blockNumber:X}", false },
                id = 1
            };

            var response = await SendRpcRequestAsync(request);
            var block = response.GetProperty("result");
            var hexTimestamp = block.GetProperty("timestamp").GetString();
            
            // Convert hex timestamp to DateTime
            var timestamp = Convert.ToInt64(hexTimestamp, 16);
            var blockTime = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
            
            _logger.LogDebug("Block {BlockNumber} timestamp: {Timestamp}", blockNumber, blockTime);
            return blockTime;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting block timestamp for block {BlockNumber}", blockNumber);
            // Fallback: estimate based on block number
            var estimatedTimestamp = DateTimeOffset.UtcNow.AddSeconds((blockNumber - await GetCurrentBlockNumberAsync()) * 12);
            return estimatedTimestamp.UtcDateTime;
        }
    }

    public async Task<bool> IsBlockReachedAsync(long targetBlock)
    {
        var currentBlock = await GetCurrentBlockNumberAsync();
        return currentBlock >= targetBlock;
    }

    public async Task<EthereumBlockInfo> GetBlockInfoAsync(long blockNumber)
    {
        try
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "eth_getBlockByNumber",
                @params = new object[] { $"0x{blockNumber:X}", false },
                id = 1
            };

            var response = await SendRpcRequestAsync(request);
            var block = response.GetProperty("result");
            
            var blockInfo = new EthereumBlockInfo
            {
                BlockNumber = blockNumber,
                Hash = block.GetProperty("hash").GetString() ?? string.Empty,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(block.GetProperty("timestamp").GetString(), 16)).UtcDateTime,
                TransactionsCount = block.GetProperty("transactions").GetArrayLength()
            };
            
            _logger.LogDebug("Block info retrieved: {BlockInfo}", blockInfo);
            return blockInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting block info for block {BlockNumber}", blockNumber);
            throw;
        }
    }

    private async Task<JsonElement> SendRpcRequestAsync(object request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(_ethereumRpcUrl, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var responseJson = JsonDocument.Parse(responseContent);
        
        if (responseJson.RootElement.TryGetProperty("error", out var error))
        {
            var errorMessage = error.GetProperty("message").GetString();
            throw new Exception($"Ethereum RPC error: {errorMessage}");
        }
        
        return responseJson.RootElement;
    }
}

public class EthereumBlockInfo
{
    public long BlockNumber { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int TransactionsCount { get; set; }
} 