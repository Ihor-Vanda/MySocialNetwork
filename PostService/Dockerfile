# Базовий образ для запуску додатка
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*

# Образ для збірки проекту
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PostService/PostService.csproj", "PostService/"]
RUN dotnet restore "PostService/PostService.csproj"
COPY . .
WORKDIR "/src/PostService"
RUN dotnet build "PostService.csproj" -c Release -o /app/build

# Публікація
FROM build AS publish
RUN dotnet publish "PostService.csproj" -c Release -o /app/publish

# Остаточний образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "PostService.dll"]
