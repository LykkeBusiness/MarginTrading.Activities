-- Import all Withdrawal operations as Activities
insert into Activities
(Id, AccountId, EventSourceId, Timestamp, Category, Event, DescriptionAttributes, RelatedIds, CorrelationId)
SELECT LOWER(REPLACE(CONVERT(NVARCHAR(128), NEWID()), '-', '')),
ah.AccountId,
ah.AccountId,
ah.ChangeTimestamp,
'CashMovement',
'AccountWithdrawalSucceeded',
CONCAT('[', '"', ABS(ah.ChangeAmount), '"', ',', '"', accounts.BaseAssetId, '"', ']'),
'[]',
ah.CorrelationId
FROM AccountHistory ah
inner join MarginTradingAccounts accounts ON ah.AccountId = accounts.Id
where ah.ReasonType = 'Withdraw'
