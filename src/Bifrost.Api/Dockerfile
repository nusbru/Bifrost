FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Bifrost.Api/Bifrost.Api.csproj", "src/Bifrost.Api/"]
COPY ["src/Bifrost.Contracts/Bifrost.Contracts.csproj", "src/Bifrost.Contracts/"]
COPY ["src/Bifrost.Core/Bifrost.Core.csproj", "src/Bifrost.Core/"]
COPY ["src/Bifrost.Infrastructure/Bifrost.Infrastructure.csproj", "src/Bifrost.Infrastructure/"]
RUN dotnet restore "./src/Bifrost.Api/Bifrost.Api.csproj"
COPY . .
WORKDIR "/src/src/Bifrost.Api"
RUN dotnet build "./Bifrost.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bifrost.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bifrost.Api.dll"]
