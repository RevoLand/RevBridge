using LinqToDB.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace RevBridge.Definitions
{
    public class SqlSettings : ILinqToDBSettings
    {
        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => "SqlServer";
        public string DefaultDataProvider => "SqlServer";

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return
                    new ConnectionStringSettings
                    {
                        Name = "Shard",
                        ProviderName = "SqlServer",
                        ConnectionString = $"Server={Properties.Settings.Default.SQL_Shard_Host};Database={Properties.Settings.Default.SQL_Shard_DBName};User Id={Properties.Settings.Default.SQL_Shard_User};Password={Properties.Settings.Default.SQL_Shard_Password};connection timeout=60;"
                    };
            }
        }
    }
}