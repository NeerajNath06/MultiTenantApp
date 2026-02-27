FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .

COPY SecurityAgencyApp.API/ SecurityAgencyApp.API/
COPY SecurityAgencyApp.Application/ SecurityAgencyApp.Application/
COPY SecurityAgencyApp.Infrastructure/ SecurityAgencyApp.Infrastructure/
COPY SecurityAgencyApp.Domain/ SecurityAgencyApp.Domain/
COPY SecurityAgencyApp.Web/ SecurityAgencyApp.Web/

RUN dotnet restore SecurityAgencyApp.Web/SecurityAgencyApp.Web.csproj

RUN dotnet publish SecurityAgencyApp.Web/SecurityAgencyApp.Web.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SecurityAgencyApp.Web.dll"]
