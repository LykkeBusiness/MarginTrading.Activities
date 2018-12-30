using System;
using System.Data.SqlClient;
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
[Id] [nvarchar](64) NOT NULL,
[AccountId] [nvarchar](64) NOT NULL,
[Instrument] [nvarchar](64) NOT NULL,
[Timestamp] [datetime] NOT NULL,
[Category] [nvarchar](64) NOT NULL,
[Event] [nvarchar](64) NOT NULL,
[DescriptionAttributes] [nvarchar](MAX) NOT NULL,
[Ids] [nvarchar](MAX) NOT NULL,
INDEX IX_{0}_Base (AccountId, Instrument, Timestamp, Category, Event)
);";
        
        private readonly string _connectionString;
        private readonly ILog _log;

        private static readonly PropertyInfo[] Properties = typeof(ActivityEntity).GetProperties();

        private static readonly string GetColumns = string.Join(",", Properties.Select(x => x.Name));

        private static readonly string GetFields = string.Join(",", Properties.Select(x => "@" + x.Name));

        private static readonly string GetUpdateClause = string.Join(",",
            Properties.Select(x => "[" + x.Name + "]=@" + x.Name));

        public ActivitiesRepository(string connectionString, ILog log)
        {
            _connectionString = connectionString;
            _log = log;
            
            using (var conn = new SqlConnection(connectionString))
            {
                try { conn.CreateTableIfDoesntExists(CreateTableScript, TableName); }
                catch (Exception ex)
                {
                    _log?.WriteErrorAsync(TableName, "CreateTableIfDoesntExists", null, ex);
                    throw;
                }
            }
        }

        public async Task AddAsync(IActivity orderBook)
        {
//            using (var conn = new SqlConnection(_connectionString))
//            {
//                try
//                {
//                    var entity = OrderExecutionOrderBookEntity.Create(orderBook);
//                    var sql = $"insert into {TableName} ({GetColumns}) values ({GetFields})";
//                    await conn.ExecuteAsync(sql, entity);
//                }
//                catch (Exception ex)
//                {
//                    var msg = $"Error {ex.Message} \n" +
//                              $"Entity <{nameof(OrderExecutionOrderBookEntity)}>: \n" +
//                              orderBook.ToJson();
//                    
//                    _log?.WriteWarning(nameof(ActivitiesRepository), nameof(AddAsync), msg);
//                    
//                    throw new Exception(msg);
//                }
//            }
        }

        
    }
}