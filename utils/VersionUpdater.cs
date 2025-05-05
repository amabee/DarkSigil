using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.IO.Compression;

namespace DarkSigil.Utils
{
  public class VersionUpdater
  {
    private static Config _config;
    private static string _repoOwner;
    private static string _repoName;
    private static string _userAgent;
    private static string _apiUrl;
    private static readonly string _backupDir = "backup";
    private static readonly string _updatesDir = "updates";
    private static readonly string _tempDir = "temp";

    static VersionUpdater()
    {
      try
      {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string configPath = Path.Combine(baseDirectory, "config", "config.json");

        _config = Config.LoadConfig(configPath);

        _repoOwner = !string.IsNullOrEmpty(_config.RepoOwner) ? _config.RepoOwner : "amabee";
        _repoName = !string.IsNullOrEmpty(_config.RepoName) ? _config.RepoName : "DarkSigil";
        _userAgent = !string.IsNullOrEmpty(_config.GITHUB_USER_AGENT) ? _config.GITHUB_USER_AGENT :
                      "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.6998.166 Safari/537.36";

        _apiUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error initializing VersionUpdater: {ex.Message}");
        _repoOwner = "amabee";
        _repoName = "DarkSigil";
        _userAgent = "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.6998.166 Safari/537.36";
        _apiUrl = $"https://api.github.com/repos/{_repoOwner}/{_repoName}/releases/latest";
      }
    }

    public static async Task<string> GetLatestVersion()
    {
      using var client = new HttpClient();
      client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);

      try
      {
        Console.WriteLine($"Fetching latest version from {_apiUrl} ...");
        var response = await client.GetStringAsync(_apiUrl);

        using var doc = JsonDocument.Parse(response);
        return doc.RootElement.GetProperty("tag_name").GetString() ?? "unknown";
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: Unable to fetch the latest version from GitHub. {ex.Message}");
        Console.ResetColor();
        return "unknown";
      }
    }

    public static async Task<bool> CheckForUpdates(bool performUpdate = false)
    {
      try
      {
        var latestVersion = await GetLatestVersion();
        if (latestVersion == "unknown")
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("Could not determine the latest version. Please check your internet connection.");
          Console.ResetColor();
          return false;
        }

        string currentVersion = AppVersion.CurrentVersion;

        Console.WriteLine($"Current version: {currentVersion}");
        Console.WriteLine($"Latest version: {latestVersion}");

        if (!string.IsNullOrEmpty(latestVersion) && IsVersionNewer(latestVersion, currentVersion))
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"🔔 A new version {latestVersion} is available!");
          Console.ResetColor();

          if (performUpdate)
          {
            Console.WriteLine("Starting download of the latest version...");
            bool updateSuccess = await DownloadAndPrepareUpdateAsync(AppDomain.CurrentDomain.BaseDirectory);

            if (updateSuccess)
            {
              Console.ForegroundColor = ConsoleColor.Green;
              Console.WriteLine("Update downloaded successfully!");
              Console.WriteLine("The application will now restart to complete the installation.");
              Console.ResetColor();

              LaunchUpdaterScript();

              return true;
            }
            else
            {
              Console.ForegroundColor = ConsoleColor.Red;
              Console.WriteLine("Failed to download the update. Please try again later.");
              Console.ResetColor();
              return false;
            }
          }
          else
          {
            Console.WriteLine("Run 'update --install' to download and install the latest version.");
          }

          return true;
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"You are using the latest version {currentVersion}.");
        Console.ResetColor();
        return false;
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error checking for updates: {ex.Message}");
        Console.ResetColor();
        return false;
      }
    }

    private static bool IsVersionNewer(string latestVersion, string currentVersion)
    {
      try
      {
        if (latestVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
          latestVersion = latestVersion.Substring(1);

        if (currentVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
          currentVersion = currentVersion.Substring(1);

        if (Version.TryParse(latestVersion, out var latest) &&
            Version.TryParse(currentVersion, out var current))
        {
          return latest > current;
        }

        return false;
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error comparing versions: {ex.Message}");
        return false;
      }
    }

    private static async Task<bool> DownloadAndPrepareUpdateAsync(string appDirectory)
    {
      using var client = new HttpClient();
      client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);

      try
      {
        var response = await client.GetStringAsync(_apiUrl);
        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;

        var assets = root.GetProperty("assets");
        if (assets.GetArrayLength() == 0)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("No assets found for the latest release.");
          Console.ResetColor();
          return false;
        }

        string zipAssetName = null;
        string zipDownloadUrl = null;

        foreach (var asset in assets.EnumerateArray())
        {
          string name = asset.GetProperty("name").GetString();
          if (name != null && name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
          {
            zipAssetName = name;
            zipDownloadUrl = asset.GetProperty("browser_download_url").GetString();
            break;
          }
        }

        if (zipDownloadUrl == null)
        {
          var asset = assets[0];
          string fileName = asset.GetProperty("name").GetString() ?? "DarkSigil.exe";
          string downloadUrl = asset.GetProperty("browser_download_url").GetString();

          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"No ZIP package found. Downloading single file: {fileName}");
          Console.WriteLine("Warning: This may not include all necessary files for a complete update.");
          Console.ResetColor();

          return await DownloadSingleFileUpdateAsync(appDirectory, downloadUrl, fileName);
        }

        Console.WriteLine($"Downloading package {zipAssetName} from {zipDownloadUrl}...");

        string updatesDir = Path.Combine(appDirectory, _updatesDir);
        Directory.CreateDirectory(updatesDir);

        string tempDir = Path.Combine(appDirectory, _tempDir);
        if (Directory.Exists(tempDir))
          Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);

        string zipFilePath = Path.Combine(updatesDir, zipAssetName);

        var fileBytes = await client.GetByteArrayAsync(zipDownloadUrl);
        await File.WriteAllBytesAsync(zipFilePath, fileBytes);

        Console.WriteLine($"Extracting update package...");
        ZipFile.ExtractToDirectory(zipFilePath, tempDir);

        CreateUpdaterScript(appDirectory, tempDir);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Downloaded and prepared update package successfully");
        Console.ResetColor();

        return true;
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error downloading update: {ex.Message}");
        Console.ResetColor();
        return false;
      }
    }

    private static async Task<bool> DownloadSingleFileUpdateAsync(string appDirectory, string downloadUrl, string fileName)
    {
      try
      {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);

        string updatesDir = Path.Combine(appDirectory, _updatesDir);
        Directory.CreateDirectory(updatesDir);

        var fileBytes = await client.GetByteArrayAsync(downloadUrl);
        string updateFilePath = Path.Combine(updatesDir, fileName);
        await File.WriteAllBytesAsync(updateFilePath, fileBytes);

        CreateSingleFileUpdaterScript(appDirectory, fileName);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Downloaded update to {updateFilePath}");
        Console.ResetColor();

        return true;
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error downloading single file update: {ex.Message}");
        Console.ResetColor();
        return false;
      }
    }

    private static void CreateSingleFileUpdaterScript(string appDirectory, string updateFileName)
    {
      string updaterScriptPath = Path.Combine(appDirectory, "update.bat");
      string sourceFile = Path.Combine(appDirectory, _updatesDir, updateFileName);
      string targetFile = Path.Combine(appDirectory, updateFileName);
      string backupDir = Path.Combine(appDirectory, _backupDir);

      Directory.CreateDirectory(backupDir);
      string backupFile = Path.Combine(backupDir, $"{updateFileName}.bak");

      string scriptContent = $@"@echo off
                                echo Waiting for application to exit...
                                timeout /t 2 /nobreak > nul
                                echo Creating backup...
                                if exist ""{targetFile}"" copy /Y ""{targetFile}"" ""{backupFile}""
                                echo Applying update...
                                copy /Y ""{sourceFile}"" ""{targetFile}""
                                echo Update complete! Starting application...
                                start """" ""{targetFile}""
                                del ""{updaterScriptPath}""
                                exit
                                ";

      File.WriteAllText(updaterScriptPath, scriptContent);
      Console.WriteLine($"Created updater script at {updaterScriptPath}");
    }

    private static void CreateUpdaterScript(string appDirectory, string tempDirectory)
    {
      string updaterScriptPath = Path.Combine(appDirectory, "update.bat");
      string backupDir = Path.Combine(appDirectory, _backupDir);

      Directory.CreateDirectory(backupDir);

      string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
      string versionBackupDir = Path.Combine(backupDir, $"v{AppVersion.CurrentVersion}_{timestamp}");

      string scriptContent = $@"@echo off
                                echo Waiting for application to exit...
                                timeout /t 2 /nobreak > nul
                                
                                echo Creating backup of current version...
                                mkdir ""{versionBackupDir}""
                                xcopy ""{appDirectory}*.dll"" ""{versionBackupDir}\"" /Y /Q
                                xcopy ""{appDirectory}*.exe"" ""{versionBackupDir}\"" /Y /Q
                                xcopy ""{appDirectory}*.json"" ""{versionBackupDir}\"" /Y /Q
                                xcopy ""{appDirectory}*.config"" ""{versionBackupDir}\"" /Y /Q
                                
                                echo Applying update...
                                xcopy ""{tempDirectory}\*.*"" ""{appDirectory}"" /E /Y /Q
                                
                                echo Cleaning up temporary files...
                                rmdir /S /Q ""{tempDirectory}""
                                
                                echo Update complete! Starting application...
                                cd ""{appDirectory}""
                                start """" ""{appDirectory}DarkSigil.exe""
                                
                                del ""{updaterScriptPath}""
                                exit
                                ";

      File.WriteAllText(updaterScriptPath, scriptContent);
      Console.WriteLine($"Created updater script at {updaterScriptPath}");
    }

    private static void LaunchUpdaterScript()
    {
      string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
      string updaterScriptPath = Path.Combine(appDirectory, "update.bat");

      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
          FileName = updaterScriptPath,
          UseShellExecute = true,
          CreateNoWindow = false
        };

        Process.Start(startInfo);

        Console.WriteLine("Exiting application to apply update...");
        Environment.Exit(0);
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error launching updater: {ex.Message}");
        Console.ResetColor();
      }
    }

    public static bool RollbackUpdate()
    {
      try
      {
        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string backupDir = Path.Combine(appDirectory, _backupDir);

        if (!Directory.Exists(backupDir))
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("No backups found to rollback to.");
          Console.ResetColor();
          return false;
        }

        var backupVersions = Directory.GetDirectories(backupDir)
                              .OrderByDescending(d => new DirectoryInfo(d).Name)
                              .ToArray();

        if (backupVersions.Length == 0)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("No backup versions found.");
          Console.ResetColor();
          return false;
        }

        string latestBackup = backupVersions[0];

        string rollbackScriptPath = Path.Combine(appDirectory, "rollback.bat");
        string scriptContent = $@"@echo off
                                echo Waiting for application to exit...
                                timeout /t 2 /nobreak > nul
                                echo Rolling back to previous version...
                                xcopy ""{latestBackup}\*.*"" ""{appDirectory}"" /E /Y /Q
                                echo Rollback complete! Starting application...
                                start """" ""{appDirectory}DarkSigil.exe""
                                del ""{rollbackScriptPath}""
                                exit
                                ";

        File.WriteAllText(rollbackScriptPath, scriptContent);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
          FileName = rollbackScriptPath,
          UseShellExecute = true,
          CreateNoWindow = false
        };

        Process.Start(startInfo);

        Console.WriteLine("Exiting application to apply rollback...");
        Environment.Exit(0);

        return true;
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error during rollback: {ex.Message}");
        Console.ResetColor();
        return false;
      }
    }
  }
}
