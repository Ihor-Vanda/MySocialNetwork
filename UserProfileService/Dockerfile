# Базовий образ для запуску додатка
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Образ для збірки проекту
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["UserProfileService/UserProfileService.csproj", "UserProfileService/"]
RUN dotnet restore "UserProfileService/UserProfileService.csproj"
COPY . .
WORKDIR "/src/UserProfileService"
RUN dotnet build "UserProfileService.csproj" -c Release -o /app/build

# Публікація
FROM build AS publish
RUN dotnet publish "UserProfileService.csproj" -c Release -o /app/publish

# Остаточний образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "UserProfileService.dll"]
