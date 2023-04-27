-- Import all Deposit operations as Activities
insert into [dbo].Activities
(Id, AccountId, EventSourceId, Timestamp, Category, Event, DescriptionAttributes, RelatedIds, CorrelationId)
SELECT LOWER(REPLACE(CONVERT(NVARCHAR(128), NEWID()), '-', '')),
ah.AccountId,
ah.AccountId,
ah.ChangeTimestamp,
'CashMovement',
'AccountDepositSucceeded',
CONCAT('[', '"', ah.ChangeAmount, '"', ',', '"', accounts.BaseAssetId, '"', ']'),
CONCAT('[', '"', ah.Id, '"', ']'),
ah.CorrelationId
FROM [dbo].AccountHistory ah
inner join [dbo].MarginTradingAccounts accounts ON ah.AccountId = accounts.Id
where ah.ReasonType = 'Deposit'
AND NOT EXISTS (
    SELECT 1
    FROM [dbo].Activities
    WHERE RelatedIds = CONCAT('[', '"', ah.Id, '"', ']')
);