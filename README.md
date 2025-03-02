# MarginTrading.Activities.Producer + MarginTrading.Activities.Broker #

Producer generating user activities.
It subscribes to different exchanges with events and generates activity entities based on information from event.

Broker subscribes to events, generated by producer and saves them to DB.

## How to use in prod env? ##

1. Pull "mt-activities-producer" or "mt-broker-activities" docker image with a corresponding tag.
2. Configure environment variables according to "Environment variables" section.
3. Put secrets.json with endpoint data including the certificate:
```json
{
  "Kestrel": {
    "EndPoints": {
      "HttpsInlineCertFile": {
        "Url": "https://*:5181", [https://*:5182 for broker]
        "Certificate": {
          "Path": "<path to .pfx file>",
          "Password": "<certificate password>"
        }
      }
    }
  }
}
```
4. Initialize all dependencies.
5. Run.

## How to run for debug? ##

1. Clone repo to some directory.
2. In MarginTrading.Activities.Producer or MarginTrading.Activities.Broker root create a appsettings.dev.json with 
settings.
3. Add environment variable "SettingsUrl": "appsettings.dev.json".
4. VPN to a corresponding env must be connected and all dependencies must be initialized.
5. Run.

### Configuration ###

Kestrel configuration may be passed through appsettings.json, secrets or environment.
All variables and value constraints are default. For instance, to set host URL the following env variable may be set:
```json
{
    "Kestrel__EndPoints__Http__Url": "http://*:5081"
}
```

### Environment variables ###

* *RESTART_ATTEMPTS_NUMBER* - number of restart attempts. If not set int.MaxValue is used.
* *RESTART_ATTEMPTS_INTERVAL_MS* - interval between restarts in milliseconds. If not set 10000 is used.
* *SettingsUrl* - defines URL of remote settings or path for local settings.

### Settings ###

Settings schema for producer:
<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./producer.json) -->
<!-- The below code snippet is automatically added from ./producer.json -->
```json
{
  "ActivitiesProducer": {
    "Consumers": {
      "MarginControl": {
        "ConnectionString": "String",
        "ConsumerCount": "Integer",
        "ExchangeName": "String"
      },
      "Orders": {
        "ConnectionString": "String",
        "ConsumerCount": "Integer",
        "ExchangeName": "String"
      },
      "Positions": {
        "ConnectionString": "String",
        "ConsumerCount": "Integer",
        "ExchangeName": "String"
      },
      "SessionActivity": {
        "ConnectionString": "String",
        "ConsumerCount": "Integer",
        "ExchangeName": "String"
      }
    },
    "Cqrs": {
      "ConnectionString": "String",
      "EnvironmentName": "String"
    },
    "Db": {
      "LogsConnString": "String",
      "StorageMode": "String"
    },
    "LegalEntitySettings": {
      "DefaultLegalEntity": "String"
    },
    "UseSerilog": "Boolean"
  },
  "APP_UID": "Integer",
  "ASPNETCORE_ENVIRONMENT": "String",
  "ENVIRONMENT": "String",
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "String"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Microsoft": "String"
    }
  },
  "MarginTradingAccountManagementServiceClient": {
    "ApiKey": {},
    "ServiceUrl": "String"
  },
  "MarginTradingSettingsServiceClient": {
    "ApiKey": "String",
    "ServiceUrl": "String"
  },
  "serilog": {
    "minimumLevel": {
      "default": "String"
    }
  },
  "TZ": "String"
}
```
<!-- MARKDOWN-AUTO-DOCS:END -->

Settings schema for broker:
<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./broker.json) -->
<!-- The below code snippet is automatically added from ./broker.json -->
```json
{
  "APP_UID": "Integer",
  "ASPNETCORE_ENVIRONMENT": "String",
  "ENVIRONMENT": "String",
  "serilog": {
    "Enrich": [
      "String"
    ],
    "minimumLevel": {
      "default": "String"
    },
    "Properties": {
      "Application": "String"
    },
    "Using": [
      "String"
    ],
    "writeTo": [
      {
        "Args": {
          "configure": [
            {
              "Args": {
                "outputTemplate": "String"
              },
              "Name": "String"
            }
          ]
        },
        "Name": "String"
      }
    ]
  },
  "TZ": "String"
}
```
<!-- MARKDOWN-AUTO-DOCS:END -->
