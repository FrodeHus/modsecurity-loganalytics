#!/bin/bash -e

export DNS_SERVER=${DNS_SERVER:-$(cat /etc/resolv.conf |grep -i '^nameserver'|head -n1|cut -d ' ' -f2)}

ENV_VARIABLES=$(awk 'BEGIN{for(v in ENVIRON) print "$"v}')
modseclog="/ModSecurityLogger -l ModSecurity -p /var/log/modsecurity/audit"
for FILE in etc/nginx/nginx.conf etc/nginx/conf.d/default.conf etc/nginx/conf.d/logging.conf etc/modsecurity.d/modsecurity-override.conf
do
    envsubst "$ENV_VARIABLES" <$FILE | sponge $FILE
done

source /opt/modsecurity/activate-rules.sh
$modseclog &
exec "$@"