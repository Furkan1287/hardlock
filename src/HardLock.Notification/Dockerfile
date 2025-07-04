FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5006

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HardLock.Notification/HardLock.Notification.csproj", "src/HardLock.Notification/"]
COPY ["src/shared/HardLock.Shared/HardLock.Shared.csproj", "src/shared/HardLock.Shared/"]
RUN dotnet restore "src/HardLock.Notification/HardLock.Notification.csproj"
COPY . .
WORKDIR "/src/src/HardLock.Notification"
RUN dotnet build "HardLock.Notification.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HardLock.Notification.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HardLock.Notification.dll"] 