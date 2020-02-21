using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using Octokit;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using NLog;

namespace Regata.Utilities
{

  public interface IUpdateManager
  {
    void CreateRelease();
    Task UploadReleaseToGithub();
    Task UpdateCurrentProject();

  }
  public class UpdateManager : IUpdateManager
  {
    public readonly string ReleaseTag;
    public readonly string ReleaseTitle;
    public readonly string ReleaseNotes;
    public readonly string PackageId;
    public readonly string Version;
    public readonly string RepositoryUrl;
    private readonly XElement XmlProj;
    public readonly Logger logger;
    private readonly string _path;
    private readonly Dictionary<int, LogLevel> VerbosityMode = new Dictionary<int, LogLevel> {
        { 1, LogLevel.Debug },
        { 2, LogLevel.Info },
        { 3, LogLevel.Error }
    };

    public UpdateManager(string project = "", int verboseLevel = 1)
    {
      GlobalDiagnosticsContext.Set("VerboseMode", VerbosityMode[verboseLevel].Name);
      logger = LogManager.GetCurrentClassLogger();
      logger.Debug("Initialisation of UpdateManager instance has begun");

      if (string.IsNullOrEmpty(project) || !File.Exists(project))
      {
        string[] projects = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");
        if (project.Any())
          project = projects[0];
        else
        {
          string msg = $"*.csproj file not found in current directory - '{Directory.GetCurrentDirectory()}'";
          logger.Error(msg);
          throw new FileNotFoundException(msg);
        }
      }

      _path = Path.GetDirectoryName(project);

      // TODO: should I check csproj file on corruption
      XmlProj = XElement.Load(project);

      try
      {
        ReleaseTag = $"v{XmlProj.Descendants("Version").First().Value}";
        ReleaseTitle = XmlProj.Descendants("PackageReleaseTitle").First().Value;
        ReleaseNotes = XmlProj.Descendants("PackageReleaseNotes").First().Value;
        RepositoryUrl = XmlProj.Descendants("RepositoryUrl").First().Value;
        PackageId = XmlProj.Descendants("PackageId").First().Value;
        Version = XmlProj.Descendants("Version").First().Value;
        logger.Debug($"{project} has parsed successfully.");
      }
      catch (InvalidOperationException)
      {
        string msg = "One of elements required for release preparation doesn't exist. See list of required elements in readme file of project";
        Console.WriteLine(msg);
        logger.Error(msg);
      }

      logger.Debug("Initialisation of UpdateManager instance has completed successfully");
    }

    void IUpdateManager.CreateRelease()
    {
      logger.Debug("Start of creation release files via squirrel.windows");

      var appSettings = ConfigurationManager.AppSettings;
      string squirrel = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), appSettings["SquirrelPath"]);
      if (!File.Exists(squirrel))
      {
        string msg = $"'{squirrel}' not found";
        logger.Error(msg);
        throw new FileNotFoundException(msg);
      }

      logger.Debug("squirrel.windows has found");

      var package = Path.Combine(_path, @"bin\Release", $"{PackageId}.{Version}.nupkg");

      if (!File.Exists(package))
      {
        string msg = $"'{package}' file not found.";
        logger.Error(msg);
        throw new FileNotFoundException(msg);
      }

      logger.Debug($"{package} has found");
      string errorMsg = "";

      using (var process = new Process())
      {
        process.StartInfo.FileName = squirrel;
        process.StartInfo.Arguments = $"{appSettings["SquirrelArgs"]} {package} -r {_path}\\Releases";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        process.Start();
        logger.Debug($"StandardOutput of process: '{process.StandardOutput.ReadToEnd()}'");
        logger.Debug($"StandardError of process: '{process.StandardError.ReadToEnd()}'");
        errorMsg = process.StandardError.ReadToEnd();
      }

      if (!string.IsNullOrEmpty(errorMsg))
      {
        logger.Error(errorMsg);
        throw new InvalidOperationException(errorMsg);
      }

      logger.Debug("Creation of release files has completed successfully");

    }
    async Task IUpdateManager.UploadReleaseToGithub()
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

    async Task IUpdateManager.UpdateCurrentProject()
    {

    }

  } //class UpdateManager
} //namespace Regata
