namespace Regata.Utilities.UpdateManager
{
  // TODO: implement update function
  // TODO: add tests
  // TODO: add CommandLineUtils
  // TODO: add roslyn static analyzer
  // FIXME: icon via csproj doesn't include into update, i.e. after build exe file with ico but after setup not

  class Program
  {
    /// <summary>
    /// Creates files for release using Squirrel.Windows and upload them to GitHub release page.
    /// All information (Repository, Versions, Title, Description) should be specified in csproj file.
    /// Example:
    /// UpdateManager
    /// will seek first csproj file in the current directory
    /// after that will generate release files from the package in bin\Release
    /// The name of package is <PackageId>.<Version>.nupkg
    /// </summary>
    /// <param name="ProjectPath">Path of csproj file. By default first csproj file in current directory.</param>
    /// <param name="VerboseMode">Specify minlevel for NLog. 1 - Debug, 2 - Info(default), 3 - Error</param>
    // static void Main(string ProjectPath = "", int VerboseMode = 2)
    static void Main(string[] args)
    {
      var upd = new UpdateManager(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
      // upd.CreateRelease();
      upd.UploadReleaseToGithub().Wait();
    }
  }
}
