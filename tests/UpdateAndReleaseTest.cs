using Xunit;
using System;
using System.Threading;
using Xunit.Abstractions;
using System.IO;
using System.Net;
using Regata.Utilities.UpdateManager;
using Octokit;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;


namespace UpdateManagerTests
{
    public class UpdateManagerFixture
    {
        public UpdateManager upd;
        public readonly string _path;
        public readonly GitHubClient _client;
        private IConfiguration Configuration { get; set; }

        public UpdateManagerFixture()
        {
            upd = new UpdateManager(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
            _path = @"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo";

            Configuration = new ConfigurationBuilder()
                          .AddUserSecrets<UpdateManager>()
                          .Build();

            _client = new GitHubClient(new ProductHeaderValue("regata-jinr"));
            var tokenAuth = new Credentials(Configuration["Settings:GitHubToken"]);
            _client.Credentials = tokenAuth;

        }
    }


    [TestCaseOrderer("UpdateManagerTests.PriorityOrderer", "tests")]
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

        [Fact, TestPriority(0)]
        public void Initialisation()
        {
            Assert.Equal("Header Test", _upd.upd.ReleaseTitle);
            Assert.Equal("Description Test", _upd.upd.ReleaseNotes);
            Assert.Equal(@"https://github.com/regata-jinr/TestAutoUpdateRepo", _upd.upd.RepositoryUrl);
        }

        [Fact, TestPriority(1)]
        public void CreateReleaseFiles()
        {
            Assert.True(Directory.Exists(_upd._path));
            Assert.True(Directory.Exists(Path.Combine(_upd._path, "Releases")));

            var dir = new DirectoryInfo(Path.Combine(_upd._path, "Releases"));
            Assert.False(dir.GetFiles().Any());

            _updMemb.CreateRelease();

            Assert.True(File.Exists(Path.Combine(_upd._path, "Releases", $"{_upd.upd.PackageId}-{_upd.upd.Version}-full.nupkg")));
            Assert.True(File.Exists(Path.Combine(_upd._path, "Releases", "Setup.exe")));
            Assert.True(File.Exists(Path.Combine(_upd._path, "Releases", "RELEASES")));
        }

        [Fact, TestPriority(2)]
        public void NothingInGithubReleasesTest()
        {
            Assert.False(_upd._client.Repository.Release.GetAll(_upd._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());
        }

        [Fact, TestPriority(3)]
        public async void CreateGitHubRelease()
        {
            await _updMemb.UploadReleaseToGithub();

            Assert.True(_upd._client.Repository.Release.GetAll(_upd._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());
        }

        [Fact, TestPriority(4)]
        public void CheckReleaseContent()
        {
            var rel = _upd._client.Repository.Release.GetLatest(_upd._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result;

            Assert.Equal(_updMemb.ReleaseTag, rel.TagName);
            Assert.Equal(_updMemb.ReleaseTitle, rel.Name);
            Assert.Equal(_updMemb.ReleaseNotes, rel.Body);

            var namesOfAssets = rel.Assets.Select(a => a.Name).ToList();

            Assert.Contains($"{_updMemb.PackageId}-{_updMemb.Version}-full.nupkg", namesOfAssets);
            Assert.Contains("Setup.exe", namesOfAssets);
            Assert.Contains("RELEASES", namesOfAssets);
        }

        [Fact, TestPriority(5)]
        public void CheckIfReleaseAlreadyExist()
        {
            Assert.Throws<InvalidOperationException>(() => _updMemb.UploadReleaseToGithub().Wait());
        }

        [Fact, TestPriority(6)]
        public void ClearAllTest()
        {
            var dir = new DirectoryInfo(Path.Combine(_upd._path, "Releases"));
            Assert.True(dir.GetFiles().Any());

            foreach (var file in dir.GetFiles())
                file.Delete(); 

            Assert.False(dir.GetFiles().Any());

            Assert.True(_upd._client.Repository.Release.GetAll(_upd._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());

            var rel = _upd._client.Repository.Release.GetLatest(_upd._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result;

            _upd._client.Repository.Release.Delete("regata-jinr", "TestAutoUpdateRepo", rel.Id).Wait();

            Assert.False(_upd._client.Repository.Release.GetAll(_upd._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());

        }

        [Fact, TestPriority(7)]
        public void CheckIfCommitsAreDifferent()
        {
            Assert.True(false);

        }


    } // public class UpdateManagerTest : IClassFixture<UpdateManagerFixture>
} // namespace Regata.Utilities.UpdateManager.Test
