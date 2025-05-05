using System;
using System.Reflection;

namespace DarkSigil
{

  public class AppVersion
  {
    /// <summary>
    /// APP VERSIONING LOGIC
    /// </summary>

    private static Config config = Config.LoadConfig("config/config.json");
    public static string CurrentVersion => Assembly
       .GetExecutingAssembly()
       .GetName()
       .Version?
       .ToString() ?? "unknown";

    public static string GetFormattedVersion()
    {
      return $"DarkSigil v{CurrentVersion}";
    }

    public static bool IsNewerThan(string versionToCompare)
    {
      if (Version.TryParse(CurrentVersion, out var current)
          && Version.TryParse(versionToCompare, out var other))
      {
        return current > other;
      }

      return false;
    }
  }
}

