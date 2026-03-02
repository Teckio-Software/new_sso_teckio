# ====== Build (.NET 10) ======
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SSO.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# ====== Runtime (.NET 10) ======
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8081 \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app ./

RUN if [ ! -f appsettings.Development.json ]; then \
        if [ -f appsettings.Template.json ]; then \
            cp appsettings.Template.json appsettings.Development.json; \
        else \
            echo "{}" > appsettings.Development.json; \
        fi \
    fi

ENTRYPOINT ["dotnet", "SSO.dll"]

EXPOSE 8081