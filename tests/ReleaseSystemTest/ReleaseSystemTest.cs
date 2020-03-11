using Xunit;
using System;
using Xunit.Abstractions;
using System.IO;
using Octokit;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Regata.Utilities.Deploy.Release.Test
{
    public class ReleaseSystemFixture
    {
        public ReleaseFactory rf;
        public readonly string _path;
        public readonly GitHubClient client;
        public readonly string owner;
        public readonly string repo;
        private IConfiguration Configuration { get; set; }

        public ReleaseSystemFixture()
        {
            rf = new ReleaseFactory(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
            _path = @"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo";

            Configuration = new ConfigurationBuilder()
                          .AddUserSecrets<ReleaseFactory>()
                          .Build();

            client = new GitHubClient(new ProductHeaderValue("regata-jinr"));
            var tokenAuth = new Credentials(Configuration["Settings:GitHubToken"]);
            client.Credentials = tokenAuth;

            owner = "regata-jinr";
            repo = "TestAutoUpdateRepo";
        }

        public Octokit.Release LatestRelease
        {
            get
            {
                return client.Repository.Release.GetLatest(owner, repo).Result;
            }
        }

        public bool IsAnyReleases
        {
            get
            {
                return client.Repository.Release.GetAll(owner, repo).Result.Any();
            }
        }
    }

    // FIXME: the order not working!
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
        public async void NothingInGithubReleasesTest()
        {
            Assert.False(_rf.IsAnyReleases);
        }

        [Fact, TestPriority(3)]
        public async void CreateGitHubRelease()
        {
            await _rfMemb.UploadReleaseToGithub();

            var rels = await _rf.client.Repository.Release.GetAll(_rf.owner, _rf.repo);
            
            Assert.True(_rf.IsAnyReleases);
        }

        [Fact, TestPriority(4)]
        public async void CheckReleaseContent()
        {
            var rel = _rf.LatestRelease;

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
            Assert.Throws<AggregateException>(() => _rfMemb.UploadReleaseToGithub().Wait());
        }

        [Fact, TestPriority(6)]
        public async void ClearAllTest()
        {
            var dir = new DirectoryInfo(Path.Combine(_rf._path, "Releases"));
            Assert.True(dir.GetFiles().Any());

            foreach (var file in dir.GetFiles())
                file.Delete();

            Assert.False(dir.GetFiles().Any());

            Assert.True(_rf.IsAnyReleases);

            var rel = _rf.LatestRelease;
            await _rf.client.Repository.Release.Delete(_rf.owner, _rf.repo, rel.Id);
            Assert.False(_rf.IsAnyReleases);

            // FIXME: deleting releases are not deleting tags!
            // looks like we can't delete tags via octokit, but we can do it via cl:
            /*
             * ➜ git ls-remote -t
             * From git@github.com:regata-jinr/TestAutoUpdateRepo.git
             * 892fb906064f74259a64890e5aa1e70dc41ddce0        refs/tags/v1.2.7
             * ➜ git push --delete origin "v1.2.7"
             * To github.com:regata-jinr/TestAutoUpdateRepo.git
             * - [deleted]         v1.2.7
             */

            //var tag = await _rf.client.Repository.GetAllTags(_rf.owner, _rf.repo);

        }

        [Fact, TestPriority(7)]
        public void CheckIfCommitsAreDifferent()
        {
            Assert.True(false);

        }

    } // public class ReleaseSystemTest : IClassFixture<ReleaseSystemFixture>
} // namespace Regata.Utilities.ReleaseSystem.Test
