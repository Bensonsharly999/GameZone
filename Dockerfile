FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/GameZone.Api/GameZone.Api.csproj src/GameZone.Api/
COPY src/GameZone.Application/GameZone.Application.csproj src/GameZone.Application/
COPY src/GameZone.Infrastructure/GameZone.Infrastructure.csproj src/GameZone.Infrastructure/
COPY src/GameZone.Domain/GameZone.Domain.csproj src/GameZone.Domain/
RUN dotnet restore src/GameZone.Api/GameZone.Api.csproj
COPY src src
RUN dotnet publish src/GameZone.Api/GameZone.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV GAMEZONE_DATA_DIR=/data
ENV ASPNETCORE_ENVIRONMENT=Production
RUN mkdir -p /data
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "GameZone.Api.dll"]
