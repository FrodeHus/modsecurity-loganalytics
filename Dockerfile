FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
ADD . .
RUN dotnet publish -c Release -o publish ModSecurityLogger/ModSecurityLogger.csproj

FROM owasp/modsecurity-crs:nginx
ENV DOTNET_RUNNING_IN_CONTAINER=true
RUN apt update && apt install -y certbot python-certbot-nginx
COPY modsec-nginx/modsec_config/ /etc/modsecurity.d/
COPY --from=build /src/publish/ModSecurityLogger .
COPY modsec-nginx/docker-entrypoint.sh .
COPY modsec-nginx/nginx/www/ /usr/share/nginx/html
RUN mkdir -p /var/log/modsecurity/audit && chown -R nginx:nginx /var/log/modsecurity
