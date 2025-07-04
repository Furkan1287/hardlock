FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HardLock.Gateway/HardLock.Gateway.csproj", "src/HardLock.Gateway/"]
COPY ["src/shared/HardLock.Shared/HardLock.Shared.csproj", "src/shared/HardLock.Shared/"]
RUN dotnet restore "src/HardLock.Gateway/HardLock.Gateway.csproj"
COPY . .
WORKDIR "/src/src/HardLock.Gateway"
RUN dotnet build "HardLock.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HardLock.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HardLock.Gateway.dll"] 