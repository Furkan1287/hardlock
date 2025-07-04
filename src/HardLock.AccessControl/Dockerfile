FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HardLock.AccessControl/HardLock.AccessControl.csproj", "src/HardLock.AccessControl/"]
COPY ["src/shared/HardLock.Shared/HardLock.Shared.csproj", "src/shared/HardLock.Shared/"]
RUN dotnet restore "src/HardLock.AccessControl/HardLock.AccessControl.csproj"
COPY . .
WORKDIR "/src/src/HardLock.AccessControl"
RUN dotnet build "HardLock.AccessControl.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HardLock.AccessControl.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HardLock.AccessControl.dll"] 