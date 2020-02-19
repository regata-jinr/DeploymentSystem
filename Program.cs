using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using Octokit;
using System.IO;

namespace Regata
{
  // TODO: complete design of update RegataUpdateManager
  // TODO: console utility with 2args package (the last created by default)
  // TODO: check squirrel.windows exists in project for update 
  // TODO: add checks for nuget packages and csproj file
  // TODO: create release via squirrel.exe 
  // TODO: upload assest via octokit
  // TODO: add tests
  // TODO: add usersecrets for tokenAuth

  class Program
  {
    static void Main(string[] args)
    {

      var x = XElement.Load("RegataUpdateManager.csproj");

      Console.WriteLine(x.Descendants("Copyright").First().Value);
      Console.WriteLine(x.Descendants("PackageReleaseNotes").First().Value);
      Console.WriteLine(x.Descendants("PackageReleaseTitle").First().Value);
    }
  }

  public class UpdateManager
  {
    public string ReleaseTag { get { return $"v{XmlProj.Descendants("Version").First().Value}"; } }
    public string ReleaseTitle { get { return XmlProj.Descendants("PackageReleaseTitle").First().Value; } }
    public string ReleaseNotes { get { return XmlProj.Descendants("PackageReleaseNotes").First().Value; } }
    private XElement XmlProj;

    public UpdateManager(string project = "")
    {
      XmlProj = XElement.Load(project);

    }

    private void CreateRelease()
    {
      string squirrel = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\.nuget\\packages\\squirrel.windows\\1.9.1\\tools\\Squirrel.exe";
      var di = new DirectoryInfo(".");
      string packagePath = di.GetFiles().OrderBy(f => f.LastWriteTime).First().FullName;

      System.Diagnostics.Process.Start(squirrel, $"--no-msi --no-delta --releasify {packagePath}");
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
  }
}
