using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using Octokit;
using System.IO;
using System.Configuration;
using NLog;

namespace Regata
{
  public class UpdateManager
  {
    public readonly string ReleaseTag;
    public readonly string ReleaseTitle;
    public readonly string ReleaseNotes;
    private readonly XElement XmlProj;
    public readonly NLog.Logger logger;
    private readonly Dictionary<int, NLog.LogLevel> VerbosityMode = new Dictionary<int, LogLevel> {
        { 1, NLog.LogLevel.Debug },
        { 2, NLog.LogLevel.Info },
        { 3, NLog.LogLevel.Error }
    };

    public UpdateManager(string project = "", int verboseLevel = 1)
    {
      NLog.GlobalDiagnosticsContext.Set("VerboseMode", VerbosityMode[verboseLevel].Name);
      logger = NLog.LogManager.GetCurrentClassLogger();
      logger.Debug("Initialisation of UpdateManager instance has begun");

      if (File.Exists(project))
        XmlProj = XElement.Load(project);
      else
      {
        Console.WriteLine($"Specified csproj file not found '{project}'");
        throw new FileNotFoundException();
      }

      try
      {
        ReleaseTag = $"v{XmlProj.Descendants("Version").First().Value}";
        ReleaseTitle = XmlProj.Descendants("PackageReleaseTitle").First().Value;
        ReleaseNotes = XmlProj.Descendants("PackageReleaseNotes").First().Value;
      }
      catch (InvalidOperationException ioe)
      {
        Console.WriteLine(ioe.Message);
      }
    }

    private void CreateRelease()
    {
      var appSettings = ConfigurationManager.AppSettings;
      string squirrel = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), appSettings["SquirrelPath"]);
      var di = new DirectoryInfo(".");
      string packagePath = di.GetFiles().OrderBy(f => f.LastWriteTime).First().FullName;

      System.Diagnostics.Process.Start(squirrel, $"{appSettings["SquirrelArgs"]} {packagePath}");
    }
    private async Task UploadReleaseToGithub()
    {
      var client = new GitHubClient(new ProductHeaderValue("bdrum"));
      var tokenAuth = new Credentials("token");
      client.Credentials = tokenAuth;

      var newRelease = new NewRelease(ReleaseTag);
      newRelease.Name = ReleaseTitle;
      newRelease.Body = ReleaseNotes;

      var result = await client.Repository.Release.Create("bdrum", "octokit.net", newRelease);
      Console.WriteLine("Created release id {0}", result.Id);

      var latestRelease = client.Repository.Release.GetLatest("bdrum", "octokit.net");

      using (var archiveContents = File.OpenRead("output.nupkg"))
      {
        var assetUpload = new ReleaseAssetUpload()
        {
          FileName = "Nupkg",
          ContentType = "package",
          RawData = archiveContents
        };
        var asset = await client.Repository.Release.UploadAsset(latestRelease.Result, assetUpload);
      }
    }

  } //class UpdateManager
} //namespace Regata
