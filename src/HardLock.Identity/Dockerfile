FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HardLock.Identity/HardLock.Identity.csproj", "src/HardLock.Identity/"]
COPY ["src/shared/HardLock.Shared/HardLock.Shared.csproj", "src/shared/HardLock.Shared/"]
COPY ["src/shared/HardLock.Security/HardLock.Security.csproj", "src/shared/HardLock.Security/"]
RUN dotnet restore "src/HardLock.Identity/HardLock.Identity.csproj"
COPY . .
WORKDIR "/src/src/HardLock.Identity"
RUN dotnet build "HardLock.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HardLock.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/HardLock.Identity/appsettings.json ./
ENTRYPOINT ["dotnet", "HardLock.Identity.dll"] 