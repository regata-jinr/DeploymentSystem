# REGATA Update Manager

> **This tool aims to unification of update process for each application from our software ecosystem.**

We use:

* [Nuget](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) for packing application
* [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows) for creation updatable application
* [Octokit](https://github.com/octokit/octokit.net) for creation [Github Releases](https://help.github.com/en/github/administering-a-repository/about-releases) that local application use for own update.
* [xUnit](https://xunit.net/) for tests
* [User-Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows) for keeping secrets

TODO List:
* to complete design of RegataUpdateManager
* check dependencies of updated project
  * squirrel.windows
* check that directory has nuget packages and csproj file
* upload assets via octokit
* add tests
* add user secrets for tokenAuth
* add usage text and detailed example

## Usage

All data should be specified in your csproj file:

~~~xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>netcoreapp3.1</TargetFramework>
  <PackageId>RegataUpdateManager</PackageId>
  <Title>RegataUpdateManager</Title>
  <Version>0.0.1</Version>
  <Authors>Boris Rumyantsev</Authors>
  <Company>REGATA, FLNP, JINR</Company>
  <PackageDescription>RegataUpdateManager</PackageDescription>
  <Copyright>2020</Copyright>
  <PackageReleaseNotes>test1</PackageReleaseNotes>
  <PackageReleaseTitle>test2</PackageReleaseTitle>
  <RepositoryUrl></RepositoryUrl>
</PropertyGroup>
~~~

