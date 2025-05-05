using System.Text.Json;

public class Config
{

  public string Version { get; set; } = "1.0.0.0";
  public string RepoOwner { get; set; } = "amabee";
  public string RepoName { get; set; } = "DarkSigil";
  public string GITHUB_USER_AGENT { get; set; } = "Mozilla/5.0 (Windows NT 11.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.6998.166 Safari/537.36";

  public Config() { }

  public static Config LoadConfig(string path)
  {
    try
    {
      if (!File.Exists(path))
      {
        Console.WriteLine($"Warning: Configuration file not found at {path}");
        return new Config();
      }

      var json = File.ReadAllText(path);
      var options = new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      };

      var config = JsonSerializer.Deserialize<Config>(json, options);
      if (config == null)
      {
        throw new InvalidOperationException("Failed to deserialize the configuration.");
      }
      return config;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error loading configuration: {ex.Message}");
      return new Config();
    }
  }
}
