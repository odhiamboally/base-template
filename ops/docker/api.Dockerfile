# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Global.json", "."]
COPY ["nuget.config", "."]
COPY ["src/Shared/BT.SharedKernel/BT.SharedKernel.csproj", "src/Shared/BT.SharedKernel/"]
COPY ["src/Shared/BT.SharedKernel.Validation/BT.SharedKernel.Validation.csproj", "src/Shared/BT.SharedKernel.Validation/"]
COPY ["src/Backend/Domain/BT.Domain/BT.Domain.csproj", "src/Backend/Domain/BT.Domain/"]
COPY ["src/Backend/Application/BT.Application/BT.Application.csproj", "src/Backend/Application/BT.Application/"]
COPY ["src/Backend/Infrastructure/BT.Infrastructure/BT.Infrastructure.csproj", "src/Backend/Infrastructure/BT.Infrastructure/"]
COPY ["src/Backend/Persistence/BT.Persistence/BT.Persistence.csproj", "src/Backend/Persistence/BT.Persistence/"]
COPY ["src/Backend/Api/BT.Api/BT.Api.csproj", "src/Backend/Api/BT.Api/"]

RUN dotnet restore "src/Backend/Api/BT.Api/BT.Api.csproj"

COPY . .

RUN dotnet publish "src/Backend/Api/BT.Api/BT.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "BT.Api.dll"]
