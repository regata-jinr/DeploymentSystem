using Octokit;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Regata.Utilities.Deploy.Release
{
  internal class Release
  {
    private readonly string _path;
    public Release(string PathOfReleases, string squirrelArgs = "")
    {
      if (!Directory.Exists(PathOfReleases))
        throw new DirectoryNotFoundException("Directory with release files not found");

      _path = PathOfReleases;
      Assets = new List<ReleaseAssetUpload>();

      AddFilesToAssetsList("*.exe");
      //TODO: find out is it necessary to add RELEASES file
      AddFilesToAssetsList("RELEASES");

      var cd = new DirectoryInfo(_path);
      var LastPckgFile = cd.GetFiles("*.nupkg").Where(f => !f.Name.Contains("-delta.nupkg")).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

      if (LastPckgFile == null)
        throw new FileNotFoundException("No one nuget-package file was found. Check your releases folder.");

      AddFilesToAssetsList(LastPckgFile.Name);

      if (!squirrelArgs.Contains("--no-delta"))
      {
        var LastDeltaPckgFile = cd.GetFiles("*.nupkg").Where(f => f.Name.Contains("-delta.nupkg")).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

        if (LastDeltaPckgFile == null)
          throw new FileNotFoundException("No one delta nuget-package file was found. Check your releases folder or add '--no-delta' to your squirrel args");

        AddFilesToAssetsList(LastDeltaPckgFile.Name);
      }

      if (!squirrelArgs.Contains("--no-msi"))
        AddFilesToAssetsList("*.msi");

    }

    public readonly IReadOnlyDictionary<string, string> _commonTypesExts = new Dictionary<string, string> {
        { ".exe", "application/exe" },
        { ".msi", "application/exe" },
        { ".nupkg", "application/zip" },
        { "", "text/plain" },
        { ".txt", "text/plain" },
        { ".json", "text/json" },
        { ".xml", "text/xml" }
    };
    public List<ReleaseAssetUpload> Assets { get; private set; }

    private void AddFilesToAssetsList(string pattern)
    {
      var files = Directory.GetFiles(_path, pattern);

      foreach (var file in files)
      {
        var assetSetup = new ReleaseAssetUpload()
        {
          FileName = file,
          ContentType = _commonTypesExts[Path.GetExtension(file)]
        };
        Assets.Add(assetSetup);
        Console.WriteLine($"'{file}' was added to assets for release.");
      }
    }

  } // class Release
} // namespace Regata.Utilities.UpdateManager
