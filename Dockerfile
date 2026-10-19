# Build a partir da raiz da solução: docker build -t aureo-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar só os .csproj primeiro aproveita o cache do restore.
COPY Aureo.Api/Aureo.Api.csproj Aureo.Api/
COPY Aureo.Application/Aureo.Application.csproj Aureo.Application/
COPY Aureo.Domain/Aureo.Domain.csproj Aureo.Domain/
COPY Aureo.Infrastructure/Aureo.Infrastructure.csproj Aureo.Infrastructure/
RUN dotnet restore Aureo.Api/Aureo.Api.csproj

COPY Aureo.Api/ Aureo.Api/
COPY Aureo.Application/ Aureo.Application/
COPY Aureo.Domain/ Aureo.Domain/
COPY Aureo.Infrastructure/ Aureo.Infrastructure/
RUN dotnet publish Aureo.Api/Aureo.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Hosts com proxy reverso (Render, Railway, Fly etc.) terminam o HTTPS fora do container.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Aureo.Api.dll"]
