dev:
	ASPNETCORE_ENVIRONMENT=Development dotnet run

prod:
	ASPNETCORE_ENVIRONMENT=Production dotnet run

test:
	dotnet test
