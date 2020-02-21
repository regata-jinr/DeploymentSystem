using Xunit;
using Xunit.Abstractions;
using System.IO;

namespace Regata.Utilities.Test
{
  public class UpdateManagerFixture
  {
    public UpdateManager upd;

    public UpdateManagerFixture()
    {
      upd = new UpdateManager(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
    }

  }
  public class UpdateManagerTest : IClassFixture<UpdateManagerFixture>
  {
    private readonly ITestOutputHelper output;
    public UpdateManagerFixture _upd;
    public IUpdateManager _iupd;

    public UpdateManagerTest(UpdateManagerFixture upd, ITestOutputHelper output)
    {
      _upd = upd;
      _iupd = _upd.upd;
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

      // Assert.False(Directory.Exists("Releases"));
      // _iupd.CreateRelease();
      // Assert.True(Directory.Exists("Releases"));
      // Directory.Delete("Releases");

    }

  }
}