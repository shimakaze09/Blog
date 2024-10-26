FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Migrate/Migrate.csproj", "Migrate/"]
RUN dotnet restore "Migrate/Migrate.csproj"
COPY . .
WORKDIR "/src/Migrate"
RUN dotnet build "Migrate.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Migrate.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Migrate.dll"]
