#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS base
WORKDIR /app
RUN apt-get update \
  && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
  && rm -rf /var/lib/apt/lists/*
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
#Configurações de variáveis de ambiente no Embrapa.io
ARG ASPNET_ENV
ARG IO_VERSION
ARG IO_STAGE
ARG GOOGLE_CLIENTID
ARG GOOGLE_CLIENTSECRET
ARG DB_HOST
ARG DB_NAME
ARG DB_USER
ARG DB_PASS
ARG MATOMO_ID
ARG MATOMO_APIURL
ARG LOG_LEVEL
ARG API_GATEWAY_USERNAME
ARG API_GATEWAY_PASSWORD
ARG API_GATEWAY_URL
ARG EMPREGADO_API_BASE_PATH
ARG UNIDADE_API_BASE_PATH
ARG JWTBEARERTOKEN_SECRETKEY
ARG JWTBEARERTOKEN_AUDIENCE
ARG JWTBEARERTOKEN_ISSUER
ARG JWTBEARERTOKEN_EXPIRETIME
ARG BASE_URL
ARG BASE_ROUTE_INTEGRACAO_API
ARG TRAEFIK_HOST
ARG SENTRY_DSN

ENV  DOTNET_NUGET_SIGNATURE_VERIFICATION=false
WORKDIR /src
COPY ["Boilerplate4Dev.csproj", "."]
RUN dotnet restore "./Boilerplate4Dev.csproj"
COPY . .
WORKDIR "/src/."

# Substituição das variaveis no arquivo de configuracao
RUN set -ex \
  && sed -i -E "s/CI_COMMIT_TAG/${IO_VERSION}/g" appsettings.json \
  && sed -i -E "s/GOOGLE_CLIENTSECRET/${GOOGLE_CLIENTSECRET}/g" appsettings.json \
  && sed -i -E "s/GOOGLE_CLIENTID/${GOOGLE_CLIENTID}/g" appsettings.json \
  && sed -i -E "s/DB_HOST/${DB_HOST}/g" appsettings.json \
  && sed -i -E "s/DB_NAME/${DB_NAME}/g" appsettings.json \
  && sed -i -E "s/DB_USER/${DB_USER}/g" appsettings.json \
  && sed -i -E "s/DB_PASS/${DB_PASS}/g" appsettings.json \
  && sed -i -E "s+MATOMO_APIURL+${MATOMO_APIURL}+g" appsettings.json \
  && sed -i -E "s/MATOMO_SITEID/${MATOMO_ID}/g" appsettings.json \
  && sed -i -E "s/MATOMO_STAGE/${IO_STAGE}/g" appsettings.json \
  && sed -i -E "s/MATOMO_VERSION/${IO_VERSION}/g" appsettings.json \
  && sed -i -E "s/LOG_LEVEL/${LOG_LEVEL}/g" appsettings.json \
  && sed -i -E "s/API_GATEWAY_USERNAME/${API_GATEWAY_USERNAME}/g" appsettings.json \
  && sed -i -E "s/API_GATEWAY_PASSWORD/${API_GATEWAY_PASSWORD}/g" appsettings.json \
  && sed -i -E "s+API_GATEWAY_URL+${API_GATEWAY_URL}+g" appsettings.json \
  && sed -i -E "s+EMPREGADO_API_BASE_PATH+${EMPREGADO_API_BASE_PATH}+g" appsettings.json \
  && sed -i -E "s+UNIDADE_API_BASE_PATH+${UNIDADE_API_BASE_PATH}+g" appsettings.json \
  && sed -i -E "s/JWTBEARERTOKEN_SECRETKEY/${JWTBEARERTOKEN_SECRETKEY}/g" appsettings.json \
  && sed -i -E "s/JWTBEARERTOKEN_AUDIENCE/${JWTBEARERTOKEN_AUDIENCE}/g" appsettings.json \
  && sed -i -E "s+JWTBEARERTOKEN_ISSUER+${JWTBEARERTOKEN_ISSUER}+g" appsettings.json \
  && sed -i -E "s/JWTBEARERTOKEN_EXPIRETIME/${JWTBEARERTOKEN_EXPIRETIME}/g" appsettings.json \
  && sed -i -E "s+BASE_URL+${BASE_URL}+g" appsettings.json \
  && sed -i -E "s+BASE_ROUTE_INTEGRACAO_API+${BASE_ROUTE_INTEGRACAO_API}+g" appsettings.json \
  && sed -i -E "s+SENTRY_DSN+${SENTRY_DSN}+g" appsettings.json


RUN dotnet build "Boilerplate4Dev.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Boilerplate4Dev.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV TZ=America/Sao_Paulo

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Boilerplate4Dev.dll"]
