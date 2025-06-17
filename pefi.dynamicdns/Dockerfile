#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM  --platform=$BUILDPLATFORM  mcr.microsoft.com/dotnet/sdk:9.0 AS build
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["pefi.dynamicdns.csproj", "pefi.dynamicdns/"]

RUN --mount=type=secret,id=github_token,env=GITHUB_TOKEN dotnet nuget add source --username petefield --password $GITHUB_TOKEN --store-password-in-clear-text --name petefield "https://nuget.pkg.github.com/petefield/index.json"

RUN dotnet restore "./pefi.dynamicdns/./pefi.dynamicdns.csproj"
COPY . pefi.dynamicdns/
WORKDIR "/src/pefi.dynamicdns"
RUN dotnet build "./pefi.dynamicdns.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./pefi.dynamicdns.csproj" -a '$TARGETARCH' -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "pefi.dynamicdns.dll"]