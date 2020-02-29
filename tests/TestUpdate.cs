using Xunit;
using Xunit.Abstractions;
using System.IO;
using Octokit;
using System.Net;


namespace Regata.Utilities.UpdateManager.Test
{
  public class UpdateManagerFixture
  {
    public UpdateManager upd;
    public readonly string _path;

    public UpdateManagerFixture()
    {
      upd = new UpdateManager(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
      _path = @"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo";
    }

  }
  public class UpdateManagerTest : IClassFixture<UpdateManagerFixture>
  {
    private readonly ITestOutputHelper output;
    public UpdateManagerFixture _upd;
    public UpdateManager _updMemb;

    public UpdateManagerTest(UpdateManagerFixture upd, ITestOutputHelper output)
    {
      _upd = upd;
      _updMemb = _upd.upd;
      this.output = output;
    }

    [Fact]
    public void Initialisation()
    {
      // Assert.Equal(_upd.upd.ReleaseTag, "");
      Assert.Equal("Header Test", _upd.upd.ReleaseTitle);
      Assert.Equal("Description Test", _upd.upd.ReleaseNotes);
      Assert.Equal("https://github.com/regata-jinr/TestAutoUpdateRepo", _upd.upd.RepositoryUrl);
    }

    [Fact]
    public void CreateReleaseFiles()
    {
      Assert.True(Directory.Exists(_upd._path));
      Assert.True(Directory.Exists(Path.Combine(_upd._path, "Releases")));
      _updMemb.CreateRelease();
      Assert.True(File.Exists(Path.Combine(_upd._path, "Releases", $"{_upd.upd.PackageId}-{_upd.upd.Version}-full.nupkg")));
    }

    [Fact]
    public void CreateGitHubRelease()
    {
      using (WebClient client = new WebClient())
      {
        string tagHtml = client.DownloadString("https://github.com/regata-jinr/TestAutoUpdateRepo/tags");
        string tagAssets = client.DownloadString("https://github.com/regata-jinr/TestAutoUpdateRepo/releases/latest");
        Assert.DoesNotContain($"tag/{_updMemb.ReleaseTag}", tagHtml);
        Assert.DoesNotContain($"download/{_updMemb.ReleaseTag}/{_updMemb.PackageId}-{_updMemb.Version}-full.nupkg", tagAssets);

        _updMemb.UploadReleaseToGithub().Wait();

        tagHtml = client.DownloadString("https://github.com/regata-jinr/TestAutoUpdateRepo/tags");
        tagAssets = client.DownloadString("https://github.com/regata-jinr/TestAutoUpdateRepo/releases/latest");
        Assert.Contains($"tag/{_updMemb.ReleaseTag}", tagHtml);
        // Assert.Contains($"download/{_updMemb.ReleaseTag}/{_updMemb.PackageId}-{_updMemb.Version}-full.nupkg", tagAssets);
      }
    }

  } // public class UpdateManagerTest : IClassFixture<UpdateManagerFixture>
} // namespace Regata.Utilities.UpdateManager.Test
