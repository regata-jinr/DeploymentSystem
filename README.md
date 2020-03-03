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
* add command line configuration
* find out is it necessary to add RELEASES file
* implement update function
* check dependencies of updated project
  * squirrel.windows
* icon via csproj doesn't include into update, i.e. after build exe file with ico but after setup not
* test coverage should be 100%
* add usage description and detailed examples

## Usage

All data should be specified in your csproj file:

~~~xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <PackageId>NameOfPackageANDRepository</PackageId>
    <Title>NameOfPackage</Title>
    <Version>1.2.7</Version>
    <Authors>REGATA</Authors>
    <Owners>Boris Rumyantsev</Owners>
    <Company>REGATA, FLNP, JINR</Company>
    <PackageDescription>Smth</PackageDescription>
    <Copyright>2020</Copyright>
    <PackageReleaseTitle>Header Test</PackageReleaseTitle>
    <PackageReleaseNotes>Description Test</PackageReleaseNotes>
    <RepositoryUrl>https://github.com/regata-jinr/TestAutoUpdateRepo</RepositoryUrl>
    <ApplicationIcon>NameOfPackage.ico</ApplicationIcon>
    <PackageIcon>NameOfPackage.png</PackageIcon>
</PropertyGroup>
~~~

