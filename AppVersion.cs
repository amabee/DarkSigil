using System;

namespace DarkSigil
{

  public class AppVersion
  {
    /// <summary>
    /// APP VERSIONING LOGIC
    /// </summary>
    
  private static Config config = Config.LoadConfig("config/config.json");
    public static string CurrentVersion => config.Version;

    public static string GetFormattedVersion()
    {
      return $"DarkSigil v{CurrentVersion}";
    }

    public static bool IsNewerThan(string versionToCompare)
    {
      var currentVersionParts = CurrentVersion.Split('.');
      var compareVersionParts = versionToCompare.Split('.');

      for (int i = 0; i < currentVersionParts.Length; i++)
      {
        int currentPart = int.Parse(currentVersionParts[i]);
        int comparePart = int.Parse(compareVersionParts[i]);

        if (currentPart > comparePart)
        {
          return true;
        }
        if (currentPart < comparePart)
        {
          return false;
        }
      }
      return false; // If all parts are equal, return false

    }
  }
}

