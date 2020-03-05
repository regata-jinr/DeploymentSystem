namespace Regata.Utilities.UpdateManager
{
  // TODO: implement update function
  // TODO: add tests
  // TODO: add roslyn static analyzer
  // FIXME: icon via csproj doesn't include into update, i.e. after build exe file with ico but after setup not
  // FIXME: System.CommandLine.DragonFruit doesn't work with xunit.runner.visualstudio and Microsoft.NET.Test.Sdk
  // https://github.com/dotnet/command-line-api/issues/809

  class Program
  {
    /// <summary>
    /// Creates files for release using Squirrel.Windows and upload them to GitHub release page.
    /// All information (Repository, Versions, Title, Description) should be specified in csproj file.
    /// Example:
    /// UpdateManager test.csproj
    /// will seek first csproj file in the current directory
    /// after that will generate release files from the package in bin\Release
    /// The name of package is 'PackageId.Version.nupkg'
    /// </summary>
    /// <param name="fileProject">Path of csproj file. By default first csproj file in the current directory.</param>
    // static void Main(string fileProject)
    static void Main(string[] args)
    {
      var upd = new UpdateManager(args[0]);
      upd.CreateRelease();
      upd.UploadReleaseToGithub().Wait();
    }

  }
}
