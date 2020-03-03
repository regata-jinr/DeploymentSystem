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
  public class UpdateManager
  {
    public readonly string ReleaseTag;
    public readonly string ReleaseTitle;
    public readonly string ReleaseNotes;
    public readonly string PackageId;
    public readonly string Version;
    public readonly string RepositoryUrl;
    private readonly string _path;
    private readonly string _nupkgPath;
    private readonly string _releasesPath;
    private readonly XElement XmlProj;
    private readonly GitHubClient _client;
    private IConfiguration Configuration { get; set; }

    // FIXME: in case of setting exists in memory provider, but not in json, it will be empty!
    private readonly IReadOnlyDictionary<string, string> _defaultSettings = new Dictionary<string, string>
    {
        {"SquirrelPath", @".nuget/packages/squirrel.windows/1.9.1/tools/Squirrel.exe"},
        {"SquirrelArgs", "--no-msi --no-delta"},
        {"DefaultReleasesPath", "Releases"},
        {"Branch", "heads/master"},
        {"PathToNupkg", @"bin\Release"}
    };

    public UpdateManager(string project = "")
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

      _nupkgPath = Path.Combine(Configuration["Settings:PathToNupkg"], $"{PackageId}.{Version}.nupkg");

      if (!File.Exists(_nupkgPath))
        _nupkgPath = Path.Combine(_path, Configuration["Settings:PathToNupkg"], $"{PackageId}.{Version}.nupkg");

      if (!File.Exists(_nupkgPath))
        throw new FileNotFoundException($"Package file '{_nupkgPath}' has not found.");
    }

    public void CreateRelease()
    {
      string squirrel = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), Configuration["Settings:SquirrelPath"]);

      if (!File.Exists(squirrel))
        throw new FileNotFoundException($"'{squirrel}' not found");

      string errorMsg = "";

      using (var process = new Process())
      {
        process.StartInfo.FileName = squirrel;
        process.StartInfo.Arguments = $"{Configuration["Settings:SquirrelArgs"]} -r {_releasesPath} --releasify {_nupkgPath}";
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

    public async Task UploadReleaseToGithub()
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
        using (var tmpStream = File.OpenRead(asset.FileName))
        {
          asset.FileName = Path.GetFileName(asset.FileName);
          asset.RawData = tmpStream;
          await _client.Repository.Release.UploadAsset(release, asset);
        }
      }
    }

    private void CommitsMatchingCheck()
    {
      var branch = _client.Git.Reference.Get(Configuration["Settings:GitHubRepoOwner"], PackageId, Configuration["Settings:Branch"]).Result;
      var lastRemoteCommit = _client.Git.Commit.Get(Configuration["Settings:GitHubRepoOwner"], PackageId, branch.Object.Sha).Result;

      var lastLocalCommitSha = "";
      // FIXME: change to the usage of ProcessInfo
      // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo?view=netcore-3.1
      using (var process = new Process())
      {
        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = @"log --format=""%H"" -n 1";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = _path;
        process.Start();
        lastLocalCommitSha = process.StandardOutput.ReadLine();
      }

      if (lastRemoteCommit.Sha != lastLocalCommitSha)
        throw new InvalidOperationException("You can't create github release in case of last local commit doesn't match with last remote commit.");
    }

    private void TagAlreadyExistCheck()
    {
      if (_client.Repository.Release.GetAll(Configuration["Settings:GitHubRepoOwner"], PackageId).Result.Where(r => r.TagName == ReleaseTag).Any())
        throw new InvalidOperationException($"Tag '{ReleaseTag}' already exist. Please change version of your assembly before build. Don't forget commit this changes.");
    }

    public async Task UpdateCurrentProject()
    {
      throw new NotImplementedException();

    }

  } //class UpdateManager
} //namespace Regata.Utilities.UpdateManager
