Web application that archives content from online video platforms (currently only YouTube).


Started as a university homework project, the code for which can be found here:
* [Backend API + MVC frontend (ASP.NET Core)](https://github.com/mipaat/icd0021-22-23-s)
* [TypeScript frontends (React & Vue)](https://github.com/mipaat/icd0006-22-23-s)

Currently WIP copying and rewriting the original project.

## DB
NB!  
Postgres' max_prepared_transactions option must be set to higher than the default 0.  
Preferably at least one per connection?

Start local dev DB:
`docker compose up db -d`

## SEQ
Example of configuring SEQ logging is in dev appsettings.  
To override the API key in user secrets without redefining the entire logging config, use the config key `Logging:Serilog:WriteTo:1:Args:apiKey`.

## Dotnet tools
`dotnet tool restore`

## Migrations
(In solution root directory)
* Add migration: `dotnet ef migrations add --project RescueTube.DAL.EF.Postgres --startup-project WebApp`
* Remove migration: `dotnet ef migrations remove --project RescueTube.DAL.EF.Postgres --startup-project WebApp`
* Update to latest migration: `dotnet ef database update --project RescueTube.DAL.EF.Postgres --startup-project WebApp`
* Add migration: `dotnet ef database update MigrationName --project RescueTube.DAL.EF.Postgres --startup-project WebApp`