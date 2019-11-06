FROM mcr.microsoft.com/dotnet/core/sdk:3.0 as build-env
WORKDIR /app

COPY . .
RUN dotnet restore MovieTimes.Service.sln -s https://api.nuget.org/v3/index.json -s http://nuget/nuget
RUN dotnet publish MovieTimes.Service.ConsoleApp/MovieTimes.Service.ConsoleApp.csproj -c Release -o /app/publish -r linux-x64

FROM mcr.microsoft.com/dotnet/core/runtime:3.0
ENV DOTNET_ENVIRONMENT=Production
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "MovieTimes.Service.ConsoleApp.dll"]
