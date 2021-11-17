// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Dapper;
using Lykke.Logs.MsSql.Extensions;
using MarginTrading.Activities.Core.Domain.Abstractions;
using MarginTrading.Activities.Core.Repositories;

namespace MarginTrading.Activities.SqlRepositories
{
    public class ActivitiesRepository : IActivitiesRepository
    {
        private const string TableName = "Activities";

        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 @"[OID] [bigint] NOT NULL IDENTITY (1,1),
[Id] [nvarchar](128) NOT NULL,
[AccountId] [nvarchar](128) NOT NULL,
[Instrument] [nvarchar](128) NULL,
[EventSourceId] [nvarchar](128) NOT NULL,
[Timestamp] [datetime] NOT NULL,
[Category] [nvarchar](128) NOT NULL,
[Event] [nvarchar](128) NOT NULL,
[DescriptionAttributes] [nvarchar](MAX) NOT NULL,
[RelatedIds] [nvarchar](MAX) NOT NULL,
INDEX IX_{0}_Base (AccountId, Instrument, EventSourceId, Timestamp, Category, Event),
INDEX IX_{0}_Id UNIQUE (Id)
);";

        private const string AddCorrelationIdScript = @"IF NOT EXISTS (
SELECT * 
    FROM   sys.columns 
    WHERE  object_id = OBJECT_ID(N'[dbo].[Activities]') 
AND name = 'CorrelationId'
    )
BEGIN
    ALTER TABLE [dbo].[Activities]
ADD CorrelationId nvarchar(250) NULL;
END";

        private const string AlterAccountIdScript = @"
BEGIN
    ALTER TABLE [dbo].[Activities]
    ALTER COLUMN AccountId [nvarchar](128) NULL;
END";
        
        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly PropertyInfo[] Properties = typeof(ActivityEntity).GetProperties();

        private static readonly string GetColumns = string.Join(",", Properties.Select(x => x.Name));

        private static readonly string GetFields = string.Join(",", Properties.Select(x => "@" + x.Name));

        public ActivitiesRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            using (var conn = new SqlConnection(connectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log.WriteError("CreateTableIfDoesntExists", $"Exception while table {TableName} creation", ex);
                    throw;
                }
            }
            
            using (var connection = new SqlConnection(connectionString)) {
                connection.Open();
                using (var command = new SqlCommand(AddCorrelationIdScript)) {
                    command.Connection = connection;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _log.WriteError("AddCorrelationIdScript", $"Exception while adding CorrelationId column", ex);
                        throw;
                    }
                }
            }
            
            using (var connection = new SqlConnection(connectionString)) {
                connection.Open();
                using (var command = new SqlCommand(AlterAccountIdScript)) {
                    command.Connection = connection;
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _log.WriteError("AlterAccountIdScript", $"Exception while altering AccountId column", ex);
                        throw;
                    }
                }
            }
        }

        public async Task InsertIfNotExist(IActivity activity)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var entity = ActivityEntity.Create(activity);
                var sql = @$"
begin
    if not exists(select 1 from {TableName} where Id = @{nameof(ActivityEntity.Id)})
    begin
        insert into {TableName} ({GetColumns}) values ({GetFields})
    end
end
";
                var rowsAffected = await conn.ExecuteAsync(sql, entity);
                if (rowsAffected != 1)
                {
                    await _log.WriteWarningAsync(
                        nameof(ActivitiesRepository),
                        nameof(InsertIfNotExist),
                        $"Activity {entity.ToJson()} not inserted in db, " +
                        $"because row with Id {entity.Id} already exists");
                }
            }
        }
    }
}