using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using Octokit;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Regata.Utilities.UpdateManager
{
  public interface IUpdateManager
  {
    void CreateRelease();
    Task UploadReleaseToGithub();
    // Task UpdateCurrentProject();

  }
  public class UpdateManager : IUpdateManager
  {
    public readonly string ReleaseTag;
    public readonly string ReleaseTitle;
    public readonly string ReleaseNotes;
    public readonly string PackageId;
    public readonly string Version;
    public readonly string RepositoryUrl;
    private readonly string _path;
    private readonly string _releasesPath;
    private readonly XElement XmlProj;
    private readonly GitHubClient _client;
    private IConfiguration Configuration { get; set; }

    private readonly IReadOnlyDictionary<string, string> _defaultSettings = new Dictionary<string, string>
    {
        {"SquirrelPath", @".nuget/packages/squirrel.windows/1.9.1/tools/Squirrel.exe"},
        {"SquirrelArgs", "--no-msi --no-delta"},
        {"DefaultReleasesPath", "Releases"},
        {"Branch", "heads/master"}

    };

    public UpdateManager(string project = "", int verboseLevel = 1)
    {

      if (string.IsNullOrEmpty(project) || !File.Exists(project))
      {
        string[] projects = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj");
        if (project.Any())
          project = projects[0];
        else
          throw new FileNotFoundException($"*.csproj file not found in current directory - '{Directory.GetCurrentDirectory()}'");
      }

      _path = Path.GetDirectoryName(project);

      Configuration = new ConfigurationBuilder()
                      .AddInMemoryCollection(_defaultSettings)
                      .SetBasePath(AppContext.BaseDirectory)
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddUserSecrets<UpdateManager>()
                      .Build();

      XmlProj = XElement.Load(project);

      _releasesPath = Configuration["Settings:DefaultReleasesPath"];

      if (!Directory.Exists(_releasesPath))
        throw new DirectoryNotFoundException($"Release directory '{_releasesPath}' was not found");

      _client = new GitHubClient(new ProductHeaderValue(Configuration["Settings:GitHubRepoOwner"]));
      var tokenAuth = new Credentials(Configuration["GitHubToken"]);
      _client.Credentials = tokenAuth;

      try
      {
        ReleaseTitle = XmlProj.Descendants("PackageReleaseTitle").First().Value;
        ReleaseNotes = XmlProj.Descendants("PackageReleaseNotes").First().Value;
        RepositoryUrl = XmlProj.Descendants("RepositoryUrl").First().Value;
        PackageId = XmlProj.Descendants("PackageId").First().Value;
        Version = XmlProj.Descendants("Version").First().Value;
        ReleaseTag = $"v{Version}";
      }
      catch (InvalidOperationException)
      {
        throw new InvalidOperationException("One of elements required for release preparation doesn't exist. See list of required elements in readme file of project");
      }
    }

    void IUpdateManager.CreateRelease()
    {
      string squirrel = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Configuration["Settings:SquirrelPath"]);

      if (!File.Exists(squirrel))
        throw new FileNotFoundException($"'{squirrel}' not found");

      var package = Path.Combine(_path, @"bin\Release", $"{PackageId}.{Version}.nupkg");

      if (!File.Exists(package))
        throw new FileNotFoundException($"'{package}' file not found.");

      string errorMsg = "";

      using (var process = new Process())
      {
        process.StartInfo.FileName = squirrel;
        process.StartInfo.Arguments = $"{Configuration["Settings:SquirrelArgs"]} -r {_releasesPath} --releasify {package}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        process.Start();
        errorMsg = process.StandardError.ReadToEnd();
        Console.WriteLine(process.StandardOutput.ReadToEnd());
      }

      if (!string.IsNullOrEmpty(errorMsg))
        throw new InvalidOperationException(errorMsg);
    }

    // TODO: check if tag already exist in github
    async Task IUpdateManager.UploadReleaseToGithub()
    {
      CommitsMatchingCheck();
      TagAlreadyExistCheck();

      var newRelease = new NewRelease(ReleaseTag)
      {
        Name = ReleaseTitle,
        Body = ReleaseNotes
      };

      var result = await _client.Repository.Release.Create(Configuration["Settings:GitHubRepoOwner"], PackageId, newRelease);
      Console.WriteLine("Created release id {0}", result.Id);

      var release = _client.Repository.Release.Get(Configuration["Settings:GitHubRepoOwner"], PackageId, result.Id).Result;

      var rel = new Release(_releasesPath, Configuration["Settings:SquirrelArgs"]);

      foreach (var asset in rel.Assets)
      {
        Console.WriteLine($"File '{asset.FileName}' has started async upload...");
        await _client.Repository.Release.UploadAsset(release, asset);
      }
    }

    private void CommitsMatchingCheck()
    {
      var branch = _client.Git.Reference.Get(Configuration["Settings:GitHubRepoOwner"], PackageId, Configuration["Settings:Branch"]).Result;
      var lastRemoteCommit = _client.Git.Commit.Get(Configuration["Settings:GitHubRepoOwner"], PackageId, branch.Object.Sha).Result;

      var lastLocalCommitSha = "";
      using (var process = new Process())
      {
        process.StartInfo.FileName = "git log";
        process.StartInfo.Arguments = @"--format=""%H"" -n 1";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
        process.Start();
        lastLocalCommitSha = process.StandardOutput.ReadToEnd();
      }

      if (lastRemoteCommit.Sha != lastLocalCommitSha)
        throw new InvalidOperationException("You can't create github release in case of last local commit doesn't match with last remote commit.");
    }

    private void TagAlreadyExistCheck()
    {
      if (_client.Repository.Release.GetAll(Configuration["Settings:GitHubRepoOwner"], PackageId).Result.Where(r => r.TagName == ReleaseTag).Any())
        throw new InvalidOperationException($"Tag '{ReleaseTag}' already exist. Please change version of your assembly before build. Don't forget commit this changes.");
    }

    // async Task IUpdateManager.UpdateCurrentProject()
    // {

    // }

  } //class UpdateManager
} //namespace Regata.Utilities.UpdateManager
