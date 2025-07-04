FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HardLock.Audit/HardLock.Audit.csproj", "src/HardLock.Audit/"]
COPY ["src/shared/HardLock.Shared/HardLock.Shared.csproj", "src/shared/HardLock.Shared/"]
RUN dotnet restore "src/HardLock.Audit/HardLock.Audit.csproj"
COPY . .
WORKDIR "/src/src/HardLock.Audit"
RUN dotnet build "HardLock.Audit.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HardLock.Audit.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HardLock.Audit.dll"] 