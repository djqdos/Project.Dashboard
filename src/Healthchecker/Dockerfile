
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

#This copies the entirety of the solution folder, but this seems like overkill. It is, however, the only way that restore works...
COPY . .

RUN dotnet restore src/Healthchecker/Healthchecker.csproj --no-cache

RUN dotnet build src/Healthchecker/Healthchecker.csproj -c Release --no-restore -p:RunAnalyzers=false

RUN dotnet publish src/Healthchecker/Healthchecker.csproj --nologo --no-restore --no-build -c Release -o /publish -p:RunAnalyzers=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --chown=appuser:appuser --from=build /publish .
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENTRYPOINT ["dotnet", "Healthchecker.dll"]