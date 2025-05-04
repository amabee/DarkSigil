using System.Text.Json;

public class Config
{
  public required string Version { get; set; }
  public required string RepoOwner { get; set; }
  public required string RepoName { get; set; }
  public required string GITHUB_USER_AGENT { get; set; }

  public static Config LoadConfig(string path)
  {
    var json = File.ReadAllText(path);
    var config = JsonSerializer.Deserialize<Config>(json);
    if (config == null)
    {
      throw new InvalidOperationException("Failed to deserialize the configuration.");
    }
    return config;
  }
}
