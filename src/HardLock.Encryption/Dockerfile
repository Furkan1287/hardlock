FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HardLock.Encryption/HardLock.Encryption.csproj", "src/HardLock.Encryption/"]
COPY ["src/shared/HardLock.Shared/HardLock.Shared.csproj", "src/shared/HardLock.Shared/"]
COPY ["src/shared/HardLock.Security/HardLock.Security.csproj", "src/shared/HardLock.Security/"]
RUN dotnet restore "src/HardLock.Encryption/HardLock.Encryption.csproj"
COPY . .
WORKDIR "/src/src/HardLock.Encryption"
RUN dotnet build "HardLock.Encryption.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HardLock.Encryption.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HardLock.Encryption.dll"] 