FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LicenseNexus.API/LicenseNexus.API.csproj", "LicenseNexus.API/"]
RUN dotnet restore "LicenseNexus.API/LicenseNexus.API.csproj"
COPY . .
WORKDIR "/src/LicenseNexus.API"
RUN dotnet build "./LicenseNexus.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LicenseNexus.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LicenseNexus.API.dll"]
