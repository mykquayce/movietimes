FROM mcr.microsoft.com/dotnet/core/sdk:latest as build-env
WORKDIR /app

COPY . .
RUN dotnet restore MovieTimes.Api.sln -s https://api.nuget.org/v3/index.json -s http://nuget/nuget
RUN dotnet publish MovieTimes.Api.WebApplication/MovieTimes.Api.WebApplication.csproj -c Release -o /app/publish -r linux-x64

FROM mcr.microsoft.com/dotnet/core/runtime:latest
ENV ASPNETCORE_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "MovieTimes.Api.WebApplication.dll"]
