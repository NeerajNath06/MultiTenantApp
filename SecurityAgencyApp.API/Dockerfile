FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY SecurityAgencyApp.API/*.csproj SecurityAgencyApp.API/
COPY SecurityAgencyApp.Application/*.csproj SecurityAgencyApp.Application/
COPY SecurityAgencyApp.Infrastructure/*.csproj SecurityAgencyApp.Infrastructure/
COPY SecurityAgencyApp.Domain/*.csproj SecurityAgencyApp.Domain/

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SecurityAgencyApp.API.dll"]
