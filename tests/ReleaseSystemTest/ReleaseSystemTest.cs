using Xunit;
using System;
using Xunit.Abstractions;
using System.IO;
using Regata.Utilities.Deploy.Release;
using Octokit;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Regata.Utilities.Deploy.Release.Test
{
    public class ReleaseSystemFixture
    {
        public ReleaseFactory rf;
        public readonly string _path;
        public readonly GitHubClient _client;
        private IConfiguration Configuration { get; set; }

        public ReleaseSystemFixture()
        {
            rf = new ReleaseFactory(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
            _path = @"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo";

            Configuration = new ConfigurationBuilder()
                          .AddUserSecrets<ReleaseFactory>()
                          .Build();

            _client = new GitHubClient(new ProductHeaderValue("regata-jinr"));
            var tokenAuth = new Credentials(Configuration["Settings:GitHubToken"]);
            _client.Credentials = tokenAuth;

        }
    }


    [TestCaseOrderer("ReleaseSystemTests.PriorityOrderer", "tests")]
    public class ReleaseSystemTest : IClassFixture<ReleaseSystemFixture>
    {
        private readonly ITestOutputHelper output;
        public ReleaseSystemFixture _rf;
        public ReleaseFactory _rfMemb;

        public ReleaseSystemTest(ReleaseSystemFixture upd, ITestOutputHelper output)
        {
            _rf = upd;
            _rfMemb = _rf.rf;
            this.output = output;
        }

        [Fact, TestPriority(0)]
        public void Initialisation()
        {
            Assert.Equal("Header Test", _rf.rf.ReleaseTitle);
            Assert.Equal("Description Test", _rf.rf.ReleaseNotes);
            Assert.Equal(@"https://github.com/regata-jinr/TestAutoUpdateRepo", _rf.rf.RepositoryUrl);
        }

        [Fact, TestPriority(1)]
        public void CreateReleaseFiles()
        {
            Assert.True(Directory.Exists(_rf._path));
            Assert.True(Directory.Exists(Path.Combine(_rf._path, "Releases")));

            var dir = new DirectoryInfo(Path.Combine(_rf._path, "Releases"));
            Assert.False(dir.GetFiles().Any());

            _rfMemb.CreateRelease();

            Assert.True(File.Exists(Path.Combine(_rf._path, "Releases", $"{_rf.rf.PackageId}-{_rf.rf.Version}-full.nupkg")));
            Assert.True(File.Exists(Path.Combine(_rf._path, "Releases", "Setup.exe")));
            Assert.True(File.Exists(Path.Combine(_rf._path, "Releases", "RELEASES")));
        }

        [Fact, TestPriority(2)]
        public void NothingInGithubReleasesTest()
        {
            Assert.False(_rf._client.Repository.Release.GetAll(_rf._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());
        }

        [Fact, TestPriority(3)]
        public async void CreateGitHubRelease()
        {
            await _rfMemb.UploadReleaseToGithub();

            Assert.True(_rf._client.Repository.Release.GetAll(_rf._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());
        }

        [Fact, TestPriority(4)]
        public void CheckReleaseContent()
        {
            var rel = _rf._client.Repository.Release.GetLatest(_rf._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result;

            Assert.Equal(_rfMemb.ReleaseTag, rel.TagName);
            Assert.Equal(_rfMemb.ReleaseTitle, rel.Name);
            Assert.Equal(_rfMemb.ReleaseNotes, rel.Body);

            var namesOfAssets = rel.Assets.Select(a => a.Name).ToList();

            Assert.Contains($"{_rfMemb.PackageId}-{_rfMemb.Version}-full.nupkg", namesOfAssets);
            Assert.Contains("Setup.exe", namesOfAssets);
            Assert.Contains("RELEASES", namesOfAssets);
        }

        [Fact, TestPriority(5)]
        public void CheckIfReleaseAlreadyExist()
        {
            Assert.Throws<InvalidOperationException>(() => _rfMemb.UploadReleaseToGithub().Wait());
        }

        [Fact, TestPriority(6)]
        public void ClearAllTest()
        {
            var dir = new DirectoryInfo(Path.Combine(_rf._path, "Releases"));
            Assert.True(dir.GetFiles().Any());

            foreach (var file in dir.GetFiles())
                file.Delete(); 

            Assert.False(dir.GetFiles().Any());

            Assert.True(_rf._client.Repository.Release.GetAll(_rf._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());

            var rel = _rf._client.Repository.Release.GetLatest(_rf._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result;

            _rf._client.Repository.Release.Delete("regata-jinr", "TestAutoUpdateRepo", rel.Id).Wait();

            Assert.False(_rf._client.Repository.Release.GetAll(_rf._client.Repository.Get("regata-jinr", "TestAutoUpdateRepo").Id).Result.Any());

        }

        [Fact, TestPriority(7)]
        public void CheckIfCommitsAreDifferent()
        {
            Assert.True(false);

        }


    } // public class ReleaseSystemTest : IClassFixture<ReleaseSystemFixture>
} // namespace Regata.Utilities.ReleaseSystem.Test
