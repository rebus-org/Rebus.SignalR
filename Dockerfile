FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
ENV PATH="$PATH:/root/.dotnet/tools"
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY Rebus.SignalR.sln ./
COPY Rebus.SignalR/Rebus.SignalR.csproj Rebus.SignalR/
COPY Rebus.SignalR.Samples/Rebus.SignalR.Samples.csproj Rebus.SignalR.Samples/
COPY Rebus.SignalR.Tests/Rebus.SignalR.Tests.csproj Rebus.SignalR.Tests/
RUN dotnet restore

# Copy everything else and run tests
COPY . .
COPY Rebus.SignalR.Samples/appsettings.docker.json Rebus.SignalR.Samples/appsettings.json
WORKDIR /src/Rebus.SignalR.Tests
RUN dotnet test -c Release

# Build
WORKDIR /src/Rebus.SignalR.Samples
RUN libman restore
RUN dotnet publish -c Release -o /app

FROM build AS publish

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Rebus.SignalR.Samples.dll"]