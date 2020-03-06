# REGATA DEPLOYMENT SYSTEM

> **These tools aim to automatisation of deployment process for each application from our software ecosystem.**

We use:

* [Nuget](https://docs.microsoft.com/en-us/nuget/reference/nuget-exe-cli-reference) for packing application
* [Squirrel.Windows](https://github.com/Squirrel/Squirrel.Windows) for creation updatable application
* [Octokit](https://github.com/octokit/octokit.net) for creation [Github Releases](https://help.github.com/en/github/administering-a-repository/about-releases) that local application use for own update.
* [xUnit](https://xunit.net/) for tests

This repo contains from few projects each cover certain feature of deployment:
* [Build](https://github.com/regata-jinr/DeploymentSystem/tree/master/ReleaseSystem/README.md) - Create files for release and upload it to github 
* [Update](https://github.com/regata-jinr/DeploymentSystem/tree/master/UpdateSystem/README.md) - Created application before running check new version on GitHub release page and update if new one
* [BackUp](https://github.com/regata-jinr/DeploymentSystem/tree/master/BackUpSystem/README.md) - We use backups for few project including our data base and spectra files


