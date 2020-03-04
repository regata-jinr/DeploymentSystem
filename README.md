# REGATA Update Manager

> **This tool aims to unification of update process for each application from our software ecosystem.**

We use:

* [Nuget](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) for packing application
* [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows) for creation updatable application
* [Octokit](https://github.com/octokit/octokit.net) for creation [Github Releases](https://help.github.com/en/github/administering-a-repository/about-releases) that local application use for own update.
* [xUnit](https://xunit.net/) for tests
* [User-Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows) for keeping secrets

TODO List:

* get release info from internal md file
* find out is it necessary to add RELEASES file
* implement update function
* check dependencies of deployed project
  * squirrel.windows
* icon via csproj doesn't include into update, i.e. after build exe file with ico but after setup not
* test coverage should be 100%

## Usage

### Preconditions

All data required for release should be specified in your csproj file.
For example:

~~~xml
<PropertyGroup>
    <PackageId>NameOfPackageANDRepository</PackageId>
    <Version>1.2.7</Version>
    <Authors>REGATA</Authors>
    <Owners>Boris Rumyantsev</Owners>
    <Company>REGATA, FLNP, JINR</Company>
    <PackageDescription>My app do that</PackageDescription>
    <Copyright>2020</Copyright>
    <PackageReleaseTitle>Header Test</PackageReleaseTitle>
    <PackageReleaseNotes>Description Test</PackageReleaseNotes>
    <RepositoryUrl>https://github.com/regata-jinr/TestAutoUpdateRepo</RepositoryUrl>
    <ApplicationIcon>NameOfPackage.ico</ApplicationIcon>
</PropertyGroup>
~~~

Also [Suirrel.Windows](https://github.com/Squirrel/Squirrel.Windows) should be installed on your machine

### Settings

You could be able to modify next parameters:

SquirrelPath - path to your squirrel.exe e.g. - ".nuget/packages/squirrel.windows/1.9.1/tools/Squirrel.exe",
SquirrelArgs - arguments for squirrel.exe e.g. - "--no-msi --no-delta",
DefaultReleasesPath - change directory for -r squirrel.windows option
GitHubRepoOwner - owner of repository with published releases. You should be the owner or collaborator.
Branch - git branch of repo by default "heads/master",
PathToNupkg - Path to nuget package for --releasify command of squirrel.windows "bin\\Release"

### Running

Imagine that you have such project,

~~~bash
TestAutoUpdateRepo
├── Program.cs
├── Releases
├── TestAutoUpdateRepo.csproj
├── TestAutoUpdateRepo.ico
├── TestAutoUpdateRepo.sln
├── bin
├── obj
└── packages
~~~

It doesn't matter that it going to do.

TestAutoUpdateRepo.csproj:

~~~xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net472</TargetFramework>
    <PackageId>TestAutoUpdateRepo</PackageId>
    <Title>TestAutoUpdateRepo</Title>
    <Version>1.2.7</Version>
    <Authors>REGATA</Authors>
    <Owners>Boris Rumyantsev</Owners>
    <Company>REGATA, FLNP, JINR</Company>
    <PackageDescription>TestAutoUpdateRepo</PackageDescription>
    <Copyright>2020</Copyright>
    <PackageReleaseTitle>Header Test</PackageReleaseTitle>
    <PackageReleaseNotes>Description Test</PackageReleaseNotes>
    <RepositoryUrl>https://github.com/regata-jinr/TestAutoUpdateRepo</RepositoryUrl>
    <ApplicationIcon>TestAutoUpdateRepo.ico</ApplicationIcon>
    <PackageIcon>TestAutoUpdateRepo.png</PackageIcon>

  </PropertyGroup>

  <Choose>
    <When Condition=" '$(Configuration)'=='Debug' ">
      <PropertyGroup>
        <DebugSymbols>true</DebugSymbols>
        <DebugType>full</DebugType>
        <Optimize>false</Optimize>
        <OutputPath>.\bin\Debug\</OutputPath>
        <DefineConstants>DEBUG;TRACE</DefineConstants>
      </PropertyGroup>
      <ItemGroup>
        <Compile Include="UnitTesting\*.cs" />
        <Reference Include="NUnit.dll" />
      </ItemGroup>
    </When>
    <When Condition=" '$(Configuration)'=='Release' ">
      <PropertyGroup>
        <DebugSymbols>false</DebugSymbols>
        <Optimize>true</Optimize>
        <OutputPath>.\bin\Release\</OutputPath>
        <DefineConstants>TRACE</DefineConstants>
        <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
      </PropertyGroup>
    </When>
  </Choose>

  <ItemGroup>
    <PackageReference Include="squirrel.windows" Version="1.9.1">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <None Include="TestAutoUpdateRepo.png" Pack="true" PackagePath=""/>
  </ItemGroup>


</Project>
~~~

As you can see, project doesn't have any releases yet:

![](https://sun9-35.userapi.com/c858120/v858120001/19973c/W6l1ApU_dNw.jpg)

Let's run this:

~~~powershell
RegataUpdateManager.exe --file-project D:\GoogleDrive\Job\flnp\dev\tests\TestAutoUpdateRepo\TestAutoUpdateRepo.csproj

Created release 'D:\...\TestAutoUpdateRepo\Releases\Setup.exe' was added to assets for release.
'D:\...\TestAutoUpdateRepo\Releases\RELEASES' was added to assets for release.
'D:\...\TestAutoUpdateRepo\Releases\TestAutoUpdateRepo-1.2.7-full.nupkg' was added to assets for release.
File 'D:\...\TestAutoUpdateRepo\Releases\Setup.exe' has started async upload ...
File 'D:\...\TestAutoUpdateRepo\Releases\Releases\RELEASES' has started async upload ...
File 'D:\...\TestAutoUpdateRepo\Releases\TestAutoUpdateRepo-1.2.7-full.nupkg' has started async upload ...
~~~

And the result will be github release:

![](https://sun9-48.userapi.com/c858120/v858120001/199759/6_AQ3XspfEc.jpg)
