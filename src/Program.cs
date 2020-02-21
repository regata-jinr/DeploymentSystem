using System;



namespace Regata.Utilities
{
  // TODO: upload assest via octokit
  // TODO: update current project method for usage in side projects
  // TODO: add tests
  // TODO: add logs
  // TODO: add usersecrets for token auth
  // TODO: generate static code analysis
  // FIXME: Log doesn't work with app.config, only if copy NLog.config to build directory manually
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
      IUpdateManager upd = new UpdateManager(@"D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
      upd.CreateRelease();
    }

  }
}
