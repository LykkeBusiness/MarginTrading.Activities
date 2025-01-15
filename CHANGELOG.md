## 2.20.1 - Nova 2. Delivery 47. Hotfix 2 (January 15, 2025)
### What's changed
* LT-5991: Bump LykkeBiz.RabbitMqBroker to 8.11.1


## 2.20.0 - Nova 2. Delivery 47 (November 15, 2024)
### What's changed
* LT-5845: Update messagepack to 2.x version.
* LT-5772: Add assembly load logger.
* LT-5741: Migrate to quorum queues.

### Deployment
In this release, all previously specified queues have been converted to quorum queues to enhance system reliability. The affected queues are:
* dev.Activities.events.exchange.MarginTrading.Activities.Broker.DefaultEnv
* lykke.mt.orderhistory.MarginTrading.Activities.Producer.dev
* lykke.mt.position.history.MarginTrading.Activities.Producer.dev
* lykke.mt.account.marginevents.MarginTrading.Activities.Producer.dev
* lykke.axle.activities.MarginTrading.Activities.Producer.dev

#### Automatic Conversion to Quorum Queues
The conversion to quorum queues will occur automatically upon service startup **if**:
* There are **no messages** in the existing queues.
* There are **no active** subscribers to the queues.

**Warning**: If messages or subscribers are present, the automatic conversion will fail. In such cases, please perform the following steps:
1. Run the previous version of the component associated with the queue.
1. Make sure all the messages are processed and the queue is empty.
1. Shut down the component associated with the queue.
1. Manually delete the existing classic queue from the RabbitMQ server.
1. Restart the component to allow it to create the quorum queue automatically.

#### Disabling Mirroring Policies
Since quorum queues inherently provide data replication and reliability, server-side mirroring policies are no longer necessary for these queues. Please disable any existing mirroring policies applied to them to prevent redundant configurations and potential conflicts.

#### Environment and Instance Identifiers
Please note that the queue names may include environment-specific identifiers (e.g., dev, test, prod). Ensure you replace these placeholders with the actual environment names relevant to your deployment. The same applies to instance names embedded within the queue names (e.g., DefaultEnv, etc.).


## 2.19.0 - Nova 2. Delivery 46 (September 26, 2024)
### What's changed
* LT-5593: Migrate to net 8.

## 2.18.0 - Nova 2. Delivery 44 (August 15, 2024)
### What's changed
* LT-5526: Update rabbitmq broker library with new rabbitmq.client and templates.

### Deployment
Please ensure that the mirroring policy is configured on the RabbitMQ server side for the following queues:
- `dev.Activities.events.exchange.MarginTrading.Activities.Broker.DefaultEnv`
- `lykke.mt.orderhistory.MarginTrading.Activities.Producer.dev`
- `lykke.mt.position.history.MarginTrading.Activities.Producer.dev`
- `lykke.mt.account.marginevents.MarginTrading.Activities.Producer.dev`
- `lykke.axle.activities.MarginTrading.Activities.Producer.dev`

These queues require the mirroring policy to be enabled as part of our ongoing initiative to enhance system reliability. They are now classified as "no loss" queues, which necessitates proper configuration. The mirroring feature must be enabled on the RabbitMQ server side.

In some cases, you may encounter an error indicating that the server-side configuration of a queue differs from the client’s expected configuration. If this occurs, please delete the queue, allowing it to be automatically recreated by the client.

**Warning**: The "no loss" configuration is only valid if the mirroring policy is enabled on the server side.

Please be aware that the provided queue names may include environment-specific identifiers (e.g., dev, test, prod). Be sure to replace these with the actual environment name in use. The same applies to instance names embedded within the queue names (e.g., DefaultEnv, etc.).

## 2.17.0 - Nova 2. Delivery 41 (March 29, 2024)
### What's changed
* LT-5444: Update packages.


## 2.16.0 - Nova 2. Delivery 40 (February 28, 2024)
### What's changed
* LT-5216: Update lykke.httpclientgenerator to 5.6.2.
* LT-5187: Add SettingsDeletedGeneralOrderSettings.


## 2.15.1 - Nova 2. Delivery 39. Hotfix 2 (February 7, 2024)
### What's changed
* LT-5234: Update vulnerable packages

## 2.15.0 - Nova 2. Delivery 39 (January 29, 2024)
### What's changed
* LT-5165: Add history of releases into `changelog.md`


## 2.14.0 - Nova 2. Delivery 37 (2023-10-17)
### What's changed
* LT-4977: Add helper methods descriptionattributeshelper.


**Full change log**: https://github.com/lykkebusiness/margintrading.activities/compare/v2.13.0...v2.14.0

## v2.13.0 - Nova 2. Delivery 36
## What's changed
* LT-4901: Update nugets.


**Full change log**: https://github.com/lykkebusiness/margintrading.activities/compare/v2.12.1...v2.13.0

## v2.12.1 - Nova 2. Delivery 34
## What's Changed

* LT-4759: Use account name in activities where applicable
* LT-4688 Extend activity entity for additional info
* LT-4735: Upgrade Lykke.MarginTrading.AssetService.Contracts

### Deployment
- Please run the query below in order to add the new column 'IsOnBehalf' to the 'dbo.Activities' table.
   - `src/MarginTrading.Activities.SqlRepositories/Scripts/LT-4688-add-isonbehalf-column.sql`

Rollback script:

```sql
ALTER TABLE dbo.Activities
DROP COLUMN IsOnBehalf;
```

**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.12.0...v2.12.1

## v2.12.0 - Nova 2. Delivery 33
## What's changed
* LT-4517: Add deposits and withdrawals as activities.
* LT-4622: Add all deposits and withdrawals data migration.
* LT-4272: To add logging for Broker publishers

### Deployment

- The following SQL scripts must be executed in order to insert activities from past Deposit and Withdrawal operations:
  - `./src/MarginTrading.Activities.SqlRepositories/Scripts/LT-4622-add-all-past-deposit-activities.sql`
  - `./src/MarginTrading.Activities.SqlRepositories/Scripts/LT-4622-add-all-past-withdrawal-activities.sql`

 


**Full change log**: https://github.com/lykkebusiness/margintrading.activities/compare/v2.10.2...v2.12.0

## v2.10.2 - Nova 2. Delivery 32
## What's changed
* LT-4401: Do not let the host keep running if startupmanager failed to start.


**Full change log**: https://github.com/lykkebusiness/margintrading.activities/compare/v2.9.2...v2.10.2

## v2.9.2 - Nova 2. Delivery 28. Hotfix 3
## What's Changed
* fix(LT-4319):  Upgrade LykkeBiz.Logs.Serilog to 3.3.1 by @gponomarev-lykke in https://github.com/LykkeBusiness/MarginTrading.Activities/pull/67


**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.9.0...v2.9.2

## v2.9.0 - Nova 2. Delivery 28
## What's Changed
* LT-3721: NET 6 migration

### Deployment
* NET 6 runtime is required
* Dockerfile is updated to use native Microsoft images (see [DockerHub](https://hub.docker.com/_/microsoft-dotnet-runtime/))

**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.8.1...v2.9.0

## v2.8.1 - Nova 2. Delivery 27
## What's Changed

* LT-4143: Update core contracts package

**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.5.0...v2.8.1

## v2.7.2 - Nova 2. Delivery 26
## What's Changed
* LT-4110: Add & handle price alerts activity type
* LT-4115: Issues with deserializing price alerts

**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.6.5...v2.7.2

## v2.6.6 - Nova 2. Delivery 23. Hotfix 5
## What's Changed
* LT-4057: New recovery tool for activities

## Deployment
* Please, read carefully instructions in `README` file of `MarginTrading.Activities.RecoveryTool` project

**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.6.4...v2.6.6

## v2.6.5 - Nova 2. Delivery 24
## What's Changed
* LT-3897: [Activities] Upgrade Lykke.HttpClientGenerator nuget by @lykke-vashetsin in https://github.com/LykkeBusiness/MarginTrading.Activities/pull/58

## New Contributors
* @lykke-vashetsin made their first contribution in https://github.com/LykkeBusiness/MarginTrading.Activities/pull/53

**Full Changelog**: https://github.com/LykkeBusiness/MarginTrading.Activities/compare/v2.6.4...v2.6.5

## v2.6.4 - Nova 2. Delivery 21
* LT-3717: NOVA security threats

## v2.6.3 - Nova 2. Delivery 19
* LT-3656: Extend ActivityType contract
* LT-3638: Cannot read messages when EventSourceId = null

### Deployment
* New version 1.1.13 of nuget package `Lykke.MarginTrading.Activities.Contracts` has been published.

## v2.6.0 - Correlation IDs
### Tasks

* LT-3609: Handle Correlation IDs

## v2.5.0 - Delivery 11.
### Tasks

* LT-3215: Create new activities

## v2.4.0 - Nova 2. Delivery 7.
### Tasks

* LT-2927: Use Liquidation events to publish Activities
* LT-2946: Handle Activities idempotent way

### Deployment

* Run script to update Schema
```sql
alter table dbo.Activities alter column Instrument nvarchar(128) NULL
```

* Run script
```sql
;with cte as
(
select
row_number() over(partition by Id order by Id ) as rn,
Id
from dbo.Activities
)
update cte set Id =newid()
where rn>1

create unique index IX_Activities_Id on dbo.Activities(Id)
sql

## v2.3.0 - Nova 2. Delivery 5.
### Tasks

* LT-2876: Add currency to activity events of type PositionPartialClosing and PositionClosing
* LT-2872: Use AccountName in Activities

### Deployment

* Run following script to migrate data
```sql
DECLARE @Rows INT,
@BatchSize INT;

SET @BatchSize = 5000;
SET @Rows = @BatchSize;

WHILE (@Rows = @BatchSize)
BEGIN

UPDATE TOP (@BatchSize) dbo.Activities
SET DescriptionAttributes = REPLACE(DescriptionAttributes,']', ',"EUR"]')
WHERE Event IN ('PositionPartialClosing', 'PositionClosing')
AND DescriptionAttributes NOT LIKE '%,"EUR"]%'
SET @Rows = @@ROWCOUNT;

END;
```

* Add to settings root if missing:
```json
"MarginTradingAccountManagementServiceClient": {
"ServiceUrl": "http://mt-account-management.mt.svc.cluster.local",
"ApiKey": ""
}
```

## v2.2.0 - Handle product start date
### Tasks 
* LT-2734: Improve asset adding workflow

## v2.1.0 - Nova 2. New ref data usage.
### Tasks

* LT-2696: Update asset pairs cache

### Deployment

Add in Producer settings
"LegalEntitySettings": {
"DefaultLegalEntity": "Default"
}

### Rabbit MQ changes

New queue: dev.Activities.queue.SettingsService.events.ProductChangedEvent.projections

## v2.0.0 - Updated Asset Service contracts
### Tasks

* LT-2397: Rename Settings Service to Asset Service. Update the contracts package.

## v1.5.5 - Bugfix
### Tasks

LT-2336: Regression - Activities is not showing any trading data

## v1.5.4 - Bugfixes
### Tasks

* LT-2281: Update Lykke.Logs.MsSql + fix component context usage

## v1.5.2 - Bugfix
### Tasks

* LT-2254: mtActivities: Multiple errors

## v1.5.0 - Migration to .NET 3.1
### Tasks

LT-2172: Migrate to 3.1 Core and update DL libraries

## v1.4.11 - Bugfixes
### Tasks

LT-2010: Improve Alpine docker files
LT-2156: Fix threads leak with RabbitMq subscribers and publishers

## v1.4.10 - Bugfixes
### Tasks

LT-2010: Improve Alpine docker files
LT-2112: Update brokerbase nuget
LT-2005: Several errors during insertion

### Deployment

Execute SQL:

````sql
ALTER TABLE Activities
ALTER COLUMN [Id] [nvarchar](128) NOT NULL;

ALTER TABLE Activities
ALTER COLUMN [AccountId] [nvarchar](128) NOT NULL;

ALTER TABLE Activities
ALTER COLUMN [Instrument] [nvarchar](128) NOT NULL;

ALTER TABLE Activities
ALTER COLUMN [EventSourceId] [nvarchar](128) NOT NULL;

ALTER TABLE Activities
ALTER COLUMN [Category] [nvarchar](128) NOT NULL;

ALTER TABLE Activities
ALTER COLUMN [Event] [nvarchar](128) NOT NULL;
````

## v1.4.7 - Alpine docker image
### Tasks

LT-1984: Migrate to Alpine docker images in MT Core services

## v1.4.6 - Http logs + improvements
### Tasks

LT-1912: MarginTrading.Activities: add WebHostLogger
LT-1856: Message resend API for not critical brokers
LT-1930: [MT-A] Update cqrs libraries
LT-1878: Incomprehensible error in MTActivities logs

## v1.4.3 - Messages resend API
### Tasks

LT-1754: Check that all services have .net core 2.2 in projects and docker
LT-1856: Message resend API for not critical brokers

## v1.4.1 - Fix wrong description of activities for MarginControl category
### Tasks:
LT-1731: fixing c unicode character

### Deployment:
- SQL must be executed:

```sql
update Activities
set Event = Replace(Event, N'С', 'C')
where Event like N'%С%'
```

## v1.4.0 - License
### Tasks
LT-1541: Update licenses in lego service to be up to latest requirements

## v1.3.3 - Improvements, bugfixes
MTC-756: Session activities updates
MTC-803: Error in mt-activities-producer after Log In
MTC-804: Errors in mt-broker-activities after SettingsChangedGeneralOrderSettingsAcknowledgementPositionClose
MTC-817: Secure all "visible" endpoints in mt-core 

### Producer deployment
Add new section to settings root (optional, if settings service does not use API key):
```json
"MarginTradingSettingsServiceClient": 
  {
    "ServiceUrl": "settings service url",
    "ApiKey": "settings service secret key"
  }
```

## v1.3.1 - Force open change + Improvements
### Tasks

MTC-745: Force open should be editable for limit/stop order
MTC-732: Broker message handling strategy must be durable
MTC-786: Do not create an activity for order when OrderChangedProperty.None

### Deployment

The broker should be redeployed in the following way:
1. Make sure the broker queue (dev.Activities.events.exchange.MarginTrading.Activities.Broker.DefaultEnv) is empty.
2. Stop the broker.
3. Go to RabbitMq dashboard and delete the queue.
4. Start a new version of the broker.

## v1.3.0 - RabbitMQ logs
### Tasks:
MTC-703: RabbitMQ logs

## v1.2.0 - Added order reject activities
### Tasks

MTC-685: Improve Activities: Rejection reason (put user friendly name instead of tech name or code)

## v1.1.4 - Session and FE activities
### Tasks

MTC-573: Activities Producer service to subscribe and listen to log in/out events
MTC-594: "Delete" accounts feature
MTC-593: Invalid activitiesproducer configuration provided
MTC-599: Activities: extend the model to keep settings type.

### Deployment

Add new settings to ActivitiesProducer -> Consumers:

````json
"SessionActivity": {
        "ConnectionString": "{rabbit mq connection string}",
        "ExchangeName": "lykke.axle.activities",
        "ConsumerCount": 2
      }
````

## v1.0.0 - Initial implementation
### Implemented features

MTC-417 : Implement activities services
MTC-552 : Finish trading activities implementation

2 new services were added.
Config examples see in README
