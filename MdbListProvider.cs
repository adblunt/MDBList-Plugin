using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Providers;

namespace MdbListRatingsProvider;

public sealed class MdbListProvider : IRemoteMetadataProvider<Movie, MovieInfo>, IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
{
    private readonly IHttpClient _httpClient;
    private readonly ILogger _logger;

    public MdbListProvider(IHttpClient httpClient, ILogManager logManager)
    {
        _httpClient = httpClient;
        _logger = logManager.GetLogger(Name);
    }

    public string Name => "MDBList Ratings Provider";

    public int Order => 0;

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<RemoteSearchResult>>(Array.Empty<RemoteSearchResult>());

    public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
        => Task.FromResult<IEnumerable<RemoteSearchResult>>(Array.Empty<RemoteSearchResult>());

    public Task<HttpResponseInfo> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return _httpClient.GetResponse(new MediaBrowser.Common.Net.HttpRequestOptions
        {
            Url = url,
            CancellationToken = cancellationToken
        });
    }

    public Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        => GetMovieMetadataInternal(info, cancellationToken);

    public Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
        => GetSeriesMetadataInternal(info, cancellationToken);

    private async Task<MetadataResult<Movie>> GetMovieMetadataInternal(MovieInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Movie>();
        var ratings = await GetRatingsAsync(info, "movie", cancellationToken).ConfigureAwait(false);
        if (ratings is null)
        {
            _logger.Info("MDBList: No ratings found for {0}", info.Name);
            return result;
        }

        _logger.Info("MDBList: Found ratings for {0} - Critic: {1}, Audience: {2}", info.Name, ratings.Critic, ratings.Audience);

        var item = new Movie();
        if (ratings.Critic.HasValue)
        {
            item.CriticRating = ratings.Critic.Value;
        }

        if (ratings.Audience.HasValue)
        {
            // Emby CommunityRating is typically 0-10. MDBList RT Audience is 0-100.
            item.CommunityRating = ratings.Audience.Value / 10f;
        }

        result.Item = item;
        result.HasMetadata = true;
        return result;
    }

    private async Task<MetadataResult<Series>> GetSeriesMetadataInternal(SeriesInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Series>();
        var ratings = await GetRatingsAsync(info, "show", cancellationToken).ConfigureAwait(false);
        if (ratings is null)
        {
            _logger.Info("MDBList: No ratings found for {0}", info.Name);
            return result;
        }

        _logger.Info("MDBList: Found ratings for {0} - Critic: {1}, Audience: {2}", info.Name, ratings.Critic, ratings.Audience);

        var item = new Series();
        if (ratings.Critic.HasValue)
        {
            item.CriticRating = ratings.Critic.Value;
        }

        if (ratings.Audience.HasValue)
        {
            // Emby CommunityRating is typically 0-10. MDBList RT Audience is 0-100.
            item.CommunityRating = ratings.Audience.Value / 10f;
        }

        result.Item = item;
        result.HasMetadata = true;
        return result;
    }

    private async Task<ParsedRatings?> GetRatingsAsync(ItemLookupInfo info, string itemType, CancellationToken cancellationToken)
    {
        var apiKey = Plugin.Instance?.Configuration?.ApiKey?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return null;
        }

        var lookup = BuildLookupInfo(info);
        if (lookup is null)
        {
            return null;
        }

        // Use RESTful URL format: https://api.mdblist.com/{provider}/{type}/{id}?apikey={apikey}
        var requestUrl = $"https://api.mdblist.com/{lookup.Value.Provider}/{itemType}/{Uri.EscapeDataString(lookup.Value.Id)}?apikey={Uri.EscapeDataString(apiKey)}";
        var maskedUrl = requestUrl.Replace(apiKey, "REDACTED");
        _logger.Info("MDBList: Requesting metadata from {0}", maskedUrl);

        HttpResponseInfo response;
        try
        {
            response = await _httpClient.GetResponse(new MediaBrowser.Common.Net.HttpRequestOptions
            {
                Url = requestUrl,
                CancellationToken = cancellationToken
            }).ConfigureAwait(false);
        }
        catch (MediaBrowser.Model.Net.HttpException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            _logger.Warn("MDBList API rate limit reached (HTTP 429). Returning empty metadata result so the next provider can run.");
            return null;
        }
        catch (MediaBrowser.Model.Net.HttpException ex)
        {
            _logger.Warn("MDBList API request failed with status {0}: {1}", ex.StatusCode, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error("MDBList API request failed: {0}", ex.Message);
            return null;
        }

        using (response)
        {
            if ((int)response.StatusCode < 200 || (int)response.StatusCode > 299)
            {
                return null;
            }

            if (response.Content is null)
            {
                return null;
            }

            string json;
            using (var reader = new StreamReader(response.Content))
            {
                json = await reader.ReadToEndAsync().ConfigureAwait(false);
            }
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var ratings = ParseRatings(json);
            if (ratings is null)
            {
                var preview = json.Length > 500 ? json.Substring(0, 500) + "..." : json;
                _logger.Warn("MDBList: Failed to parse ratings from JSON: {0}", preview);
            }

            return ratings;
        }
    }

    private static MdbLookup? BuildLookupInfo(ItemLookupInfo info)
    {
        if (TryGetProviderId(info, "Imdb", out var imdb))
        {
            return new MdbLookup("imdb", imdb);
        }

        if (TryGetProviderId(info, "Tmdb", out var tmdb))
        {
            return new MdbLookup("tmdb", tmdb);
        }

        if (TryGetProviderId(info, "Tvdb", out var tvdb))
        {
            return new MdbLookup("tvdb", tvdb);
        }

        return null;
    }

    private record struct MdbLookup(string Provider, string Id);

    private static bool TryGetProviderId(ItemLookupInfo info, string key, out string value)
    {
        value = string.Empty;
        if (info.ProviderIds is null)
        {
            return false;
        }

        if (!info.ProviderIds.TryGetValue(key, out var providerId))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(providerId))
        {
            return false;
        }

        value = providerId;
        return true;
    }

    private static ParsedRatings? ParseRatings(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        float? critic = null;
        float? audience = null;

        // 1. Try to find in the 'ratings' array (most reliable)
        if (root.TryGetProperty("ratings", out var ratingsElement) && ratingsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in ratingsElement.EnumerateArray())
            {
                if (element.TryGetProperty("source", out var sourceElement) && sourceElement.ValueKind == JsonValueKind.String)
                {
                    var source = sourceElement.GetString();
                    if (string.Equals(source, "tomatoes", StringComparison.OrdinalIgnoreCase))
                    {
                        critic = TryReadFloat(element, "value") ?? TryReadFloat(element, "score");
                    }
                    else if (string.Equals(source, "tomatoes_audience", StringComparison.OrdinalIgnoreCase) || string.Equals(source, "popcorn", StringComparison.OrdinalIgnoreCase))
                    {
                        audience = TryReadFloat(element, "value") ?? TryReadFloat(element, "score");
                    }
                }
            }
        }

        // 2. Fallback to root properties
        critic ??= TryReadFloat(root, "rt_score");
        audience ??= TryReadFloat(root, "rt_audience_score");

        if (!critic.HasValue && !audience.HasValue)
        {
            return null;
        }

        return new ParsedRatings(critic, audience);
    }

    private static float? TryReadFloat(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var valueElement))
        {
            return null;
        }

        return ReadFloat(valueElement);
    }

    private static float? TryReadFloat(JsonElement root, string parentProperty, string childProperty)
    {
        if (!root.TryGetProperty(parentProperty, out var parentElement))
        {
            return null;
        }

        if (parentElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!parentElement.TryGetProperty(childProperty, out var valueElement))
        {
            return null;
        }

        return ReadFloat(valueElement);
    }

    private static float? ReadFloat(JsonElement valueElement)
    {
        if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetSingle(out var numeric))
        {
            return numeric;
        }

        if (valueElement.ValueKind == JsonValueKind.String)
        {
            var str = valueElement.GetString();
            if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }

    private sealed record ParsedRatings(float? Critic, float? Audience);
}
