using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Runtime.InteropServices;


namespace DarkSigil.Utils
{
  public class VersionUpdater
  {
    private static Config config = Config.LoadConfig("config/config.json");

    private static string RepoOwner = config.RepoOwner;
    private static string RepoName = config.RepoName;
    private static string ApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
    private static string UserAgent = config.GITHUB_USER_AGENT;

    public static async Task<string> GetLatestVersion()
    {
      using var client = new HttpClient();
      client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

      try
      {
        Console.WriteLine($"Fetching latest version from {RepoName}...");
        var response = await client.GetStringAsync(ApiUrl);

        using var doc = JsonDocument.Parse(response);
        return doc.RootElement.GetProperty("tag_name").GetString() ?? "unknown";
      }
      catch (System.Exception)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: Unable to fetch the latest version from GitHub. Please contact the owner to notify of the error. Thanks!");
        Console.ResetColor();
        throw;
      }
    }

    public static async Task CheckForUpdates()
    {
      var latestVersion = await GetLatestVersion();

      if (!string.IsNullOrEmpty(latestVersion) && AppVersion.IsNewerThan(latestVersion))
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🔔 A new version {latestVersion} is available! Please update to the latest version.");
        Console.WriteLine($"Run 'update' to update to the latest version.");
        Console.ResetColor();

        Console.WriteLine("Do you want to download the latest version? (y/n)");

        var key = Console.ReadKey(true).KeyChar;
        if (key == 'y' || key == 'Y')
        {
          await DownloadLatestReleaseAsync(Directory.GetCurrentDirectory());
        }
        else
        {
          Console.WriteLine("Update cancelled.");
        }

        return;
      }

      if (!string.IsNullOrEmpty(latestVersion) && !AppVersion.IsNewerThan(latestVersion))
      {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"You are using the latest version {AppVersion.CurrentVersion}.");
        Console.ResetColor();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
        return;
      }
    }


    public static async Task DownloadLatestReleaseAsync(string downloadPath)
    {
      using var client = new HttpClient();
      client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

      try
      {
        var response = await client.GetStringAsync(ApiUrl);
        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;

        var assets = root.GetProperty("assets");
        if (assets.GetArrayLength() == 0)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("No assets found for the latest release.");
          Console.ResetColor();
          return;
        }

        var asset = assets[0]; // Get the first asset
        var downloadUrl = asset.GetProperty("browser_download_url").GetString();
        var fileName = asset.GetProperty("name").GetString() ?? "DarkSigil";

        Console.WriteLine($"Downloading {fileName} from {downloadUrl}...");

        var fileBytes = await client.GetByteArrayAsync(downloadUrl);
        string fullPath = Path.Combine(downloadPath, fileName);

        await File.WriteAllBytesAsync(fullPath, fileBytes);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Downloaded {fileName} to {fullPath}.");
        Console.ResetColor();
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
        throw;
      }
    }
  }
}
