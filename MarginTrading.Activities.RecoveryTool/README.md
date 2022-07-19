## MarginTrading.Activities.RecoveryTool

### Purpose

This app parses logs from **Activities** and **Mt Core** services and saves missed activity events to the database (the *dbo.Activities* table).

Order, Position, MarginControl events are pulled from **MT Core** service log.
All other supported events are pulled from **Activities.Producer** service log.

### Limitations

- The app copy-pastes parts of code from  **MarginTrading.Activities.Producer**, meaning that any changes to that service will not be reflected in this tool automatically.
- Events for activity categories **Settings** and **Session** are not logged and therefore cannot be restored.
- Since **MarginTrading.Activities.Producer** generates new id for every activity event, every run of the app will generate new events in the db, leading to duplication.
- Currently the app requires logs from both **Activities Producer** and **MT Core** services. If any of those logs are not found, an exception will be thrown.

### Log file preparation

- Determine the exact period (date and time) of the incident by querying the *dbo.Activities* table
- Get logs from **Activities Producer** and **MT Core** services
- Manually strip the parts of the log files that are outside of the incident period. **WARNING**: If log files contain any data that already has been inserted into the database, the app will insert it again, leading to duplication!!!

### Configuration

To configure the app, add *appsettings.json* file to the working directory.

```json
    {
        "Logging": {
            "LogLevel": {
                "Default": "Debug",
                "System": "Information",
                "Microsoft": "Information"
            }
        },
        "ConnectionStrings": {
            "db": ""
        },
        "ActivityProducerLogDirectory": "./activity",
        "TradingCoreLogDirectory": "./core",
        "MarginTradingAccountManagementServiceClient":
        {
            "ServiceUrl": "http://mt-account-management.mt.svc.cluster.local",
            "ApiKey": ""
        },
        "MarginTradingSettingsServiceClient":
        {
            "ServiceUrl": "http://mt-asset-service.mt.svc.cluster.local",
            "ApiKey": ""
        },
        "DryRun": true
    }
```
- *ConnectionStrings__db*: a connection string to the db that will be used to save activity events
- *ActivityProducerLogDirectory*: a directory containing logs from the **ActivityProducer** service. Multiple log files are supported. The directory must contain at least 1 log file.
- *TradingCoreLogDirectory*: a directory containing logs from the **MT Core** service. Multiple log files are supported. The directory must contain at least 1 log file.
- *MarginTradingAccountManagementServiceClient*: configuration for **Account management** service. Required for some event handlers.
- *MarginTradingSettingsServiceClient*: configuration for **Asset** service. Required for some event handlers.
- *DryRun*: if true, the events will not be stored in the database. Log files still will be parsed and prepared. This setting should be used to ensure everything is configured correctly.

### How to run

- Build the app (.net core 3.1 is required)
- Add *appsettings.json* and configure the app
- Prepare log files from **Activities.Producer** and **MT Core** services and put them into the corresponding directories
- Disable *DryRun* mode for production run
- Execute command
`dotnet MarginTrading.Activities.RecoveryTool.dll `