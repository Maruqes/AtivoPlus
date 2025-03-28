dev:
	ASPNETCORE_URLS=http://0.0.0.0:5000 ASPNETCORE_ENVIRONMENT=Development dotnet run

prod:
	ASPNETCORE_URLS=http://0.0.0.0:5000 ASPNETCORE_ENVIRONMENT=Production dotnet run

teste:
	ASPNETCORE_URLS=http://0.0.0.0:5000 ASPNETCORE_ENVIRONMENT=Development dotnet test

build:
	dotnet build

dev_sv:
	dotnet build
	ASPNETCORE_URLS=http://0.0.0.0:5000 ASPNETCORE_ENVIRONMENT=Development  dotnet bin/Debug/net9.0/AtivoPlus.dll

prod_sv:
	dotnet build
	ASPNETCORE_URLS=http://0.0.0.0:5000 ASPNETCORE_ENVIRONMENT=Production  dotnet bin/Debug/net9.0/AtivoPlus.dll