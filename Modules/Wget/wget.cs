using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DarkSigil.Interface;

namespace DarkSigil.Modules.Wget
{
  public class Wget : ICommands
  {
    private CancellationTokenSource _animationCts;
    private bool _downloadComplete;

    public void Execute(string[] args)
    {
      if (args.Length == 0)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Usage: wget <url> [options]");
        Console.WriteLine("Options: -O (output file), -P (directory), -r (recursive), -nc (no clobber), -q (quiet), -c (continue), -b (background), -U (user-agent), --header (custom header), --referer (referer), --no-check-certificate");
        Console.ResetColor();
        return;
      }

      string url = null;
      string outputFile = null;
      string outputDir = null;
      bool recursive = false;
      bool noClobber = false;
      bool quiet = false;
      bool continueDownload = false;
      bool background = false;
      string userAgent = null;
      string referer = null;
      bool noCheckCertificate = false;
      List<string> customHeaders = new();

      for (int i = 0; i < args.Length; i++)
      {
        switch (args[i])
        {
          case "-O": if (i + 1 < args.Length) outputFile = args[++i]; break;
          case "-P": if (i + 1 < args.Length) outputDir = args[++i]; break;
          case "-r": recursive = true; break;
          case "-nc": noClobber = true; break;
          case "-q": quiet = true; break;
          case "-c": continueDownload = true; break;
          case "-b": background = true; break;
          case "-U": if (i + 1 < args.Length) userAgent = args[++i]; break;
          case "--header": if (i + 1 < args.Length) customHeaders.Add(args[++i]); break;
          case "--referer": if (i + 1 < args.Length) referer = args[++i]; break;
          case "--no-check-certificate": noCheckCertificate = true; break;
          default: if (url == null && !args[i].StartsWith("-")) url = args[i]; break;
        }
      }

      if (url == null)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error: No URL specified.");
        Console.ResetColor();
        return;
      }

      if (background)
      {
        Console.WriteLine("Background download is not implemented in this version.");
        return;
      }

      _downloadComplete = false;
      _animationCts = new CancellationTokenSource();

      if (!quiet)
      {
        Task.Run(() => ShowLoadingAnimation(_animationCts.Token, quiet));
      }

      try
      {
        if (recursive)
        {
          DownloadRecursive(url, outputDir, noClobber, quiet, continueDownload, userAgent, referer, noCheckCertificate, customHeaders).Wait();
        }
        else
        {
          DownloadFile(url, outputFile, outputDir, noClobber, quiet, continueDownload, userAgent, referer, noCheckCertificate, customHeaders).Wait();
        }
      }
      finally
      {
        _downloadComplete = true;
        _animationCts.Cancel();
        Thread.Sleep(100); // Give animation thread time to complete
        Console.WriteLine(); // Ensure we move to a new line after animation
      }
    }

    private void ShowLoadingAnimation(CancellationToken cancellationToken, bool quiet)
    {
      if (quiet) return;

      string[] frames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
      int frameIndex = 0;
      int counter = 0;
      string statusText = "Downloading";

      try
      {
        while (!cancellationToken.IsCancellationRequested)
        {
          counter++;
          if (counter % 10 == 0)
          {
            statusText = statusText.Length < 13 ? statusText + "." : "Downloading";
          }

          Console.Write($"\r{frames[frameIndex]} {statusText}");
          frameIndex = (frameIndex + 1) % frames.Length;
          Thread.Sleep(80); // Fast animation
        }

        if (_downloadComplete)
        {
          Console.Write("\rDownload complete!   ");
        }
      }
      catch
      {
        // Ignore any errors in the animation thread
      }
    }

    private async Task DownloadRecursive(string url, string outputDir, bool noClobber, bool quiet, bool continueDownload, string userAgent, string referer, bool noCheckCertificate, List<string> customHeaders, HashSet<string> visited = null, HashSet<string> disallowed = null)
    {
      visited ??= new HashSet<string>();
      if (visited.Contains(url)) return;
      visited.Add(url);

      if (disallowed == null)
      {
        disallowed = await GetDisallowedPaths(url);
      }

      var uriPath = new Uri(url).AbsolutePath;
      if (disallowed.Any(d => uriPath.StartsWith(d)))
      {
        if (!quiet)
          Console.WriteLine($"\rSkipping {url} due to robots.txt");
        return;
      }

      try
      {
        using HttpClientHandler handler = new();
        if (noCheckCertificate)
          handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        using HttpClient client = new(handler);
        if (!string.IsNullOrEmpty(userAgent)) client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        if (!string.IsNullOrEmpty(referer)) client.DefaultRequestHeaders.Referrer = new Uri(referer);
        foreach (var header in customHeaders)
        {
          var parts = header.Split(new[] { ':' }, 2);
          if (parts.Length == 2) client.DefaultRequestHeaders.Add(parts[0].Trim(), parts[1].Trim());
        }

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();

        string relativePath = new Uri(url).LocalPath.TrimStart('/');
        if (string.IsNullOrEmpty(relativePath) || relativePath.EndsWith("/"))
          relativePath += "index.html";

        string fullPath = Path.Combine(outputDir ?? ".", relativePath);
        var directoryPath = Path.GetDirectoryName(fullPath);
        if (directoryPath != null)
        {
          Directory.CreateDirectory(directoryPath);
        }

        if (noClobber && File.Exists(fullPath))
        {
          if (!quiet) Console.WriteLine($"\rSkipping {fullPath}, already exists.");
        }
        else
        {
          await File.WriteAllTextAsync(fullPath, content);
          if (!quiet) Console.WriteLine($"\rDownloaded {url} to {fullPath}");
        }

        await Task.Delay(500);
        var links = ExtractLinks(content, url);
        foreach (var link in links)
        {
          await DownloadRecursive(link, outputDir, noClobber, quiet, continueDownload, userAgent, referer, noCheckCertificate, customHeaders, visited, disallowed);
        }
      }
      catch (Exception ex)
      {
        if (!quiet)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"\rFailed to download {url}: {ex.Message}");
          Console.ResetColor();
        }
      }
    }

    private static async Task<HashSet<string>> GetDisallowedPaths(string rootUrl)
    {
      HashSet<string> disallowed = new();
      try
      {
        Uri uri = new Uri(rootUrl);
        string robotsUrl = $"{uri.Scheme}://{uri.Host}/robots.txt";
        using HttpClient client = new();
        var content = await client.GetStringAsync(robotsUrl);
        foreach (var line in content.Split('\n'))
        {
          var trimmed = line.Trim();
          if (trimmed.StartsWith("Disallow:"))
          {
            string path = trimmed.Substring("Disallow:".Length).Trim();
            if (!string.IsNullOrEmpty(path))
              disallowed.Add(path);
          }
        }
      }
      catch { }
      return disallowed;
    }

    private List<string> ExtractLinks(string html, string baseUrl)
    {
      List<string> links = new();
      Uri baseUri = new Uri(baseUrl);

      string[] patterns =
      {
        "<a\\s[^>]*?href\\s*=\\s*[\"'](?<url>[^\"']+)[\"'][^>]*?>",
        "<img\\s[^>]*?src\\s*=\\s*[\"'](?<url>[^\"']+)[\"']",
        "<script\\s[^>]*?src\\s*=\\s*[\"'](?<url>[^\"']+)[\"']",
        "<link\\s[^>]*?href\\s*=\\s*[\"'](?<url>[^\"']+)[\"']"
      };

      foreach (string pattern in patterns)
      {
        var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
          string tag = match.Value;
          if (tag.Contains("rel=\"nofollow\"", StringComparison.OrdinalIgnoreCase))
            continue;

          string href = match.Groups["url"].Value;
          if (href.StartsWith("#") || href.StartsWith("mailto:") || href.StartsWith("javascript:"))
            continue;

          try
          {
            Uri uri = new Uri(baseUri, href);
            if (uri.Host == baseUri.Host)
              links.Add(uri.ToString());
          }
          catch { }
        }
      }

      return links.Distinct().ToList();
    }

    private async Task DownloadFile(string url, string outputFile, string outputDir, bool noClobber, bool quiet, bool continueDownload, string userAgent, string referer, bool noCheckCertificate, List<string> customHeaders)
    {
      try
      {
        using HttpClientHandler handler = new();
        if (noCheckCertificate)
        {
          handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        using HttpClient client = new(handler);
        if (!string.IsNullOrEmpty(userAgent)) client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        if (!string.IsNullOrEmpty(referer)) client.DefaultRequestHeaders.Referrer = new Uri(referer);
        foreach (var header in customHeaders)
        {
          var parts = header.Split(new[] { ':' }, 2);
          if (parts.Length == 2) client.DefaultRequestHeaders.Add(parts[0].Trim(), parts[1].Trim());
        }

        HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        if (string.IsNullOrEmpty(outputFile))
        {
          outputFile = Path.GetFileName(new Uri(url).AbsolutePath);
          if (string.IsNullOrEmpty(outputFile)) outputFile = "downloaded_file";
        }

        if (!string.IsNullOrEmpty(outputDir))
        {
          Directory.CreateDirectory(outputDir);
          outputFile = Path.Combine(outputDir, outputFile);
        }

        if (noClobber && File.Exists(outputFile))
        {
          if (!quiet)
            Console.WriteLine($"\rFile '{outputFile}' already exists. Skipping download due to -nc option.");
          return;
        }

        long totalBytes = response.Content.Headers.ContentLength ?? -1;
        long downloadedBytes = 0;

        using var fs = new FileStream(outputFile, continueDownload ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None);
        using var stream = await response.Content.ReadAsStreamAsync();

        byte[] buffer = new byte[8192];
        int bytesRead;
        DateTime lastUpdate = DateTime.Now;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
          await fs.WriteAsync(buffer, 0, bytesRead);
          downloadedBytes += bytesRead;

          
          if (totalBytes > 0 && !quiet && DateTime.Now - lastUpdate > TimeSpan.FromMilliseconds(250))
          {
            lastUpdate = DateTime.Now;
            double percentage = (double)downloadedBytes / totalBytes * 100;
           
          }
        }

        if (!quiet)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"\rDownloaded to {outputFile}");
          Console.ResetColor();
        }
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\rError: {ex.Message}");
        Console.ResetColor();
      }
    }
  }
}
