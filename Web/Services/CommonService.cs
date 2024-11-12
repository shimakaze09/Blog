using System.Net;
using Contrib.Utils;

namespace Web.Services;

/// <summary>
/// Some common services
/// </summary>
public class CommonService
{
    private readonly ILogger<CommonService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CommonService(ILogger<CommonService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Download a file
    /// </summary>
    /// <param name="url">URL of the file to download</param>
    /// <param name="savePath">Save path (must be a complete path)</param>
    /// <returns>The name of the downloaded file, or null if the download failed</returns>
    public async Task<string?> DownloadFile(string url, string savePath)
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var resp = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            var fileName = GuidUtils.GuidTo16String() + Path.GetExtension(url);
            var filePath = Path.Combine(savePath, WebUtility.UrlEncode(fileName));
            await using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            await resp.Content.CopyToAsync(fs);

            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error downloading file, information: {Error}", ex);
            return null;
        }
    }
}