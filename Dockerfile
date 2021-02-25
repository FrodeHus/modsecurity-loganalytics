FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
ADD . .
RUN dotnet publish -c Release -o publish ModSecurityLogger/ModSecurityLogger.csproj

FROM owasp/modsecurity-crs:nginx
COPY modsec-nginx/modsec_config/* /etc/modsecurity.d/
COPY --from=build /src/publish/ModSecurityLogger .
COPY modsec-nginx/docker-entrypoint.sh .
RUN mkdir -p /var/log/modsecurity/audit && chown nginx:nginx /var/log/modsecurity/audit