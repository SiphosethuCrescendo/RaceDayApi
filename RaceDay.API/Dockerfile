FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RaceDay.API/RaceDay.API.csproj", "RaceDay.API/"]
COPY ["RaceDay.Core/RaceDay.Core.csproj", "RaceDay.Core/"]
COPY ["RaceDay.Infrastructure/RaceDay.Infrastructure.csproj", "RaceDay.Infrastructure/"]
RUN dotnet restore "./RaceDay.API/RaceDay.API.csproj"
COPY . .
WORKDIR "/src/RaceDay.API"
RUN dotnet build "./RaceDay.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RaceDay.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.API.dll"]