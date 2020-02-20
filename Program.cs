using System;
using System.Xml.Linq;
using System.Linq;

namespace Regata
{
  // TODO: complete design of update RegataUpdateManager
  // TODO: console utility with 2args package (the last created by default)
  // TODO: check squirrel.windows exists in project for update 
  // TODO: add checks for nuget packages and csproj file
  // TODO: create release via squirrel.exe 
  // TODO: upload assest via octokit
  // TODO: add tests
  // TODO: add logs
  // TODO: add usersecrets for tokenAuth
  // TODO: generate static code analysis

  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("RegataUpdateManager under development");

      var x = XElement.Load("RegataUpdateManager.csproj");
      try
      {
        Console.WriteLine(x.Descendants("Copyright1").First().Value);
        Console.WriteLine(x.Descendants("PackageReleaseNotes").First().Value);
        Console.WriteLine(x.Descendants("PackageReleaseTitle").First().Value);
      }
      catch (InvalidOperationException ioe)
      {
        Console.WriteLine(ioe.Data);
        Console.WriteLine(ioe.Message);
        Console.WriteLine(ioe.Source);
        Console.WriteLine(ioe.StackTrace);
        Console.WriteLine(ioe.TargetSite);
        Console.WriteLine(ioe.TargetSite);
      }
    }

  } // class Program
} //namespace Regata
