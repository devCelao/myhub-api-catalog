# syntax=docker/dockerfile:1.7-labs

ARG DOTNET_VERSION=9.0
ARG BUILD_CONFIGURATION=Release

######## RUNTIME ########
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

######## BUILD ########
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar .csproj dos Componentes (repo myhub-api-all-components)
COPY myhub-api-all-components/DomainCore/DomainObjects.csproj                myhub-api-all-components/DomainCore/
COPY myhub-api-all-components/DataTransferObjects/DataTransferObjects.csproj myhub-api-all-components/DataTransferObjects/
COPY myhub-api-all-components/MessageBus/MessageBus.csproj                   myhub-api-all-components/MessageBus/
COPY myhub-api-all-components/SecurityCore/SecurityCore.csproj               myhub-api-all-components/SecurityCore/
COPY myhub-api-all-components/MicroserviceCore/MicroserviceCore.csproj       myhub-api-all-components/MicroserviceCore/
COPY myhub-api-all-components/IntegrationHandlers/IntegrationHandlers.csproj myhub-api-all-components/IntegrationHandlers/

# Copiar .csproj do serviço Catalog (repo myhub-api-catalog)
COPY myhub-api-catalog/CatalogAPI/CatalogAPI.csproj                       myhub-api-catalog/CatalogAPI/
COPY myhub-api-catalog/CatalogApplication/CatalogApplication.csproj       myhub-api-catalog/CatalogApplication/
COPY myhub-api-catalog/CatalogDomain/CatalogDomain.csproj                 myhub-api-catalog/CatalogDomain/
COPY myhub-api-catalog/CatalogInfrastructure/CatalogInfrastructure.csproj myhub-api-catalog/CatalogInfrastructure/

# Restore
RUN dotnet restore myhub-api-catalog/CatalogAPI/CatalogAPI.csproj

# Copiar todo o código-fonte
COPY myhub-api-all-components/ ./myhub-api-all-components/
COPY myhub-api-catalog/ ./myhub-api-catalog/

# Build
WORKDIR /src/myhub-api-catalog/CatalogAPI
RUN dotnet publish CatalogAPI.csproj -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

######## FINAL ########
FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogAPI.dll"]
