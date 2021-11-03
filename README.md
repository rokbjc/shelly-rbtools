# shelly-rbtools
This repo contains tools related to [Alltreco's Shelly](https://shelly.cloud/) devices. It is a work in progress. Initial intention is to create terrrafom provider for Shelly devices, but there is long way to go.

## Folders

### `/shelly-api-docs.shelly.cloud`

Contains Gen1 documentation file of Shelly devices downloaded from https://shelly-api-docs.shelly.cloud/gen1/.

### `/dotnet`

Contains sources written in .NET Core.



### `/swagger`

Contains generated swagger files grouped by device type.
### `/swagger-ui`

Contains [UI](swagger-ui/index.html) for generated swagger definitions.



## Future plans

- Finish/optimize generation of Gen1 swagger definitions

- Generate REST clients in [Go](golang.org)

- Create [Terraform](https://www.terraform.io/) provider for managing Shelly devices.

  



