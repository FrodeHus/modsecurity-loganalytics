# ModSecurity logger for Azure Log Analytics

Simple console application that monitors a folder and sends off ModSecurity logs to Azure Log Analytics (buffered - every 2 minutes).

Only supports JSON-formatted ModSecurity logs.

```text
ModSecurityLogger 1.0.0
Copyright (C) 2021 ModSecurityLogger

  -w, --workspace    ID of your workspace

  -l, --log          Required. Name of the datasource/log you wish to send data to

  -k, --key          Access key secret

  -p, --path         Required. Where to look for logfiles

  --help             Display this help screen.

  --version          Display version information.
```

Workspace ID and Access key can also be specified using environment variables WORKSPACE_ID and WORKSPACE_SHARED_KEY.

## LogAnalytics.Client

This repo also contains a Log Analytics API client which can be used standalone.

## The most prevalent OWASP rule violations

Once the logs are in Log Analytics Workspace, further analysis can be done and alert rules set up in Azure Sentinel - but one fun visualization is to see which OWASP rules are violated most often.

Run this query:

```kql
OpenWAF_CL
| order by TimeGenerated desc
| project t=todynamic(transaction_messages_s)
| mv-apply t on
(    extend file=tostring(t.details.file)
    | project rule=extract(@"([A-Z0-9\-]+)\.conf", 1, file)
)
| summarize count() by rule
|render piechart 
```

to get this:

![Most violated OWASP rules](docs/owasp-modsec.png)
