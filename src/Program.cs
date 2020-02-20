using System;

namespace Regata.Utilities
{
  // TODO: complete design of update RegataUpdateManager
  // TODO: console utility with 2args package (the last created by default)
  // TODO: check squirrel.windows exists in project for update 
  // TODO: add checks for nuget packages and csproj file
  // TODO: create release via squirrel.exe 
  // TODO: upload assest via octokit
  // TODO: update current project method for usage in side projects
  // TODO: add tests
  // TODO: add logs
  // TODO: add usersecrets for tokenAuth
  // TODO: generate static code analysis

  class Program
  {
    static void Main(string[] args)
    {
      // throw new NotImplementedException("RegataUpdateManager under development");
      IUpdateManager upd = new UpdateManager(@"D:\GoogleDrive\Job\flnp\dev\RegataUpdateManager\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj");
      upd.CreateRelease();
    }

  }
}
