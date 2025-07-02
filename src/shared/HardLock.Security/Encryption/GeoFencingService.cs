using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace HardLock.Security.Encryption;

public interface IGeoFencingService
{
    Task<bool> IsLocationAllowedAsync(GeoLocation userLocation, GeoFencingRules rules);
    Task<GeoFencingValidationResult> ValidateAccessAsync(GeoLocation userLocation, GeoFencingRules rules);
    Task<List<GeoRegion>> GetAvailableRegionsAsync();
    Task<GeoLocation> GetLocationFromIPAsync(string ipAddress);
    Task<GeoLocation> GetLocationFromCoordinatesAsync(double latitude, double longitude);
}

public class GeoFencingService : IGeoFencingService
{
    private readonly ILogger<GeoFencingService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _ipGeolocationApiUrl;

    public GeoFencingService(
        ILogger<GeoFencingService> logger,
        HttpClient httpClient,
        string ipGeolocationApiUrl = "http://ip-api.com/json")
    {
        _logger = logger;
        _httpClient = httpClient;
        _ipGeolocationApiUrl = ipGeolocationApiUrl;
    }

    public async Task<bool> IsLocationAllowedAsync(GeoLocation userLocation, GeoFencingRules rules)
    {
        try
        {
            if (!rules.IsEnabled)
            {
                _logger.LogDebug("Geo-fencing is disabled, access allowed");
                return true;
            }

            var validation = await ValidateAccessAsync(userLocation, rules);
            return validation.IsAllowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking geo-fencing access");
            return false; // Fail secure: deny access if validation fails
        }
    }

    public async Task<GeoFencingValidationResult> ValidateAccessAsync(GeoLocation userLocation, GeoFencingRules rules)
    {
        var result = new GeoFencingValidationResult
        {
            UserLocation = userLocation,
            ValidationTime = DateTime.UtcNow
        };

        try
        {
            if (!rules.IsEnabled)
            {
                result.IsAllowed = true;
                result.Reason = "Geo-fencing disabled";
                return result;
            }

            // Check country restrictions
            if (rules.AllowedCountries?.Any() == true)
            {
                var userCountry = await GetCountryFromCoordinatesAsync(userLocation.Latitude, userLocation.Longitude);
                if (!rules.AllowedCountries.Contains(userCountry, StringComparer.OrdinalIgnoreCase))
                {
                    result.IsAllowed = false;
                    result.Reason = $"Country {userCountry} not allowed";
                    result.Details = $"Allowed countries: {string.Join(", ", rules.AllowedCountries)}";
                    return result;
                }
            }

            // Check city restrictions
            if (rules.AllowedCities?.Any() == true)
            {
                var userCity = await GetCityFromCoordinatesAsync(userLocation.Latitude, userLocation.Longitude);
                if (!rules.AllowedCities.Contains(userCity, StringComparer.OrdinalIgnoreCase))
                {
                    result.IsAllowed = false;
                    result.Reason = $"City {userCity} not allowed";
                    result.Details = $"Allowed cities: {string.Join(", ", rules.AllowedCities)}";
                    return result;
                }
            }

            // Check radius-based restrictions
            if (rules.AllowedLocations?.Any() == true)
            {
                var withinRadius = false;
                foreach (var allowedLocation in rules.AllowedLocations)
                {
                    var distance = CalculateDistance(userLocation, allowedLocation.Location);
                    if (distance <= allowedLocation.RadiusMeters)
                    {
                        withinRadius = true;
                        break;
                    }
                }

                if (!withinRadius)
                {
                    result.IsAllowed = false;
                    result.Reason = "Location outside allowed radius";
                    result.Details = $"User location: {userLocation.Latitude}, {userLocation.Longitude}";
                    return result;
                }
            }

            // Check polygon-based restrictions
            if (rules.AllowedPolygons?.Any() == true)
            {
                var withinPolygon = false;
                foreach (var polygon in rules.AllowedPolygons)
                {
                    if (IsPointInPolygon(userLocation, polygon))
                    {
                        withinPolygon = true;
                        break;
                    }
                }

                if (!withinPolygon)
                {
                    result.IsAllowed = false;
                    result.Reason = "Location outside allowed regions";
                    return result;
                }
            }

            result.IsAllowed = true;
            result.Reason = "Location access granted";
            _logger.LogInformation("Geo-fencing validation passed for location: {Latitude}, {Longitude}", 
                userLocation.Latitude, userLocation.Longitude);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during geo-fencing validation");
            result.IsAllowed = false;
            result.Reason = "Validation error";
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<List<GeoRegion>> GetAvailableRegionsAsync()
    {
        // Predefined regions for common use cases
        return new List<GeoRegion>
        {
            new GeoRegion
            {
                Name = "Turkey",
                Type = GeoRegionType.Country,
                Value = "TR",
                Description = "Republic of Turkey"
            },
            new GeoRegion
            {
                Name = "Istanbul",
                Type = GeoRegionType.City,
                Value = "Istanbul",
                Description = "Istanbul Metropolitan Area"
            },
            new GeoRegion
            {
                Name = "Ankara",
                Type = GeoRegionType.City,
                Value = "Ankara",
                Description = "Ankara Metropolitan Area"
            },
            new GeoRegion
            {
                Name = "Izmir",
                Type = GeoRegionType.City,
                Value = "Izmir",
                Description = "Izmir Metropolitan Area"
            },
            new GeoRegion
            {
                Name = "Europe",
                Type = GeoRegionType.Continent,
                Value = "EU",
                Description = "European Union Countries"
            },
            new GeoRegion
            {
                Name = "North America",
                Type = GeoRegionType.Continent,
                Value = "NA",
                Description = "North American Countries"
            }
        };
    }

    public async Task<GeoLocation> GetLocationFromIPAsync(string ipAddress)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ipGeolocationApiUrl}/{ipAddress}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var geoData = JsonSerializer.Deserialize<IPGeolocationResponse>(json);
                
                if (geoData?.Status == "success")
                {
                    return new GeoLocation
                    {
                        Latitude = geoData.Lat,
                        Longitude = geoData.Lon,
                        Country = geoData.Country,
                        City = geoData.City,
                        IPAddress = ipAddress
                    };
                }
            }

            _logger.LogWarning("Failed to get location from IP: {IPAddress}", ipAddress);
            return new GeoLocation { IPAddress = ipAddress };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location from IP: {IPAddress}", ipAddress);
            return new GeoLocation { IPAddress = ipAddress };
        }
    }

    public async Task<GeoLocation> GetLocationFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var country = await GetCountryFromCoordinatesAsync(latitude, longitude);
            var city = await GetCityFromCoordinatesAsync(latitude, longitude);

            return new GeoLocation
            {
                Latitude = latitude,
                Longitude = longitude,
                Country = country,
                City = city
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location from coordinates: {Lat}, {Lng}", latitude, longitude);
            return new GeoLocation
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }
    }

    private async Task<string> GetCountryFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ipGeolocationApiUrl}?lat={latitude}&lon={longitude}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var geoData = JsonSerializer.Deserialize<IPGeolocationResponse>(json);
                return geoData?.Country ?? "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async Task<string> GetCityFromCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ipGeolocationApiUrl}?lat={latitude}&lon={longitude}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var geoData = JsonSerializer.Deserialize<IPGeolocationResponse>(json);
                return geoData?.City ?? "Unknown";
            }
            return "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private double CalculateDistance(GeoLocation point1, GeoLocation point2)
    {
        const double earthRadius = 6371000; // Earth's radius in meters

        var lat1Rad = point1.Latitude * Math.PI / 180;
        var lat2Rad = point2.Latitude * Math.PI / 180;
        var deltaLatRad = (point2.Latitude - point1.Latitude) * Math.PI / 180;
        var deltaLonRad = (point2.Longitude - point1.Longitude) * Math.PI / 180;

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    private bool IsPointInPolygon(GeoLocation point, List<GeoLocation> polygon)
    {
        if (polygon.Count < 3) return false;

        var inside = false;
        var j = polygon.Count - 1;

        for (var i = 0; i < polygon.Count; i++)
        {
            if (((polygon[i].Latitude > point.Latitude) != (polygon[j].Latitude > point.Latitude)) &&
                (point.Longitude < (polygon[j].Longitude - polygon[i].Longitude) * (point.Latitude - polygon[i].Latitude) / 
                 (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }
}

public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? IPAddress { get; set; }
}

public class GeoFencingRules
{
    public bool IsEnabled { get; set; } = false;
    public List<string>? AllowedCountries { get; set; }
    public List<string>? AllowedCities { get; set; }
    public List<GeoRadius>? AllowedLocations { get; set; }
    public List<List<GeoLocation>>? AllowedPolygons { get; set; }
}

public class GeoRadius
{
    public GeoLocation Location { get; set; } = new();
    public double RadiusMeters { get; set; }
}

public class GeoFencingValidationResult
{
    public bool IsAllowed { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ErrorMessage { get; set; }
    public GeoLocation UserLocation { get; set; } = new();
    public DateTime ValidationTime { get; set; }
}

public class GeoRegion
{
    public string Name { get; set; } = string.Empty;
    public GeoRegionType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public enum GeoRegionType
{
    Country,
    City,
    Continent,
    Custom
}

public class IPGeolocationResponse
{
    public string Status { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public double Lat { get; set; }
    public double Lon { get; set; }
} 