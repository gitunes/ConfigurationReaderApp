using System.Data;
using System.Data.SQLite;
using StackExchange.Redis;

namespace ConfigurationApp;

public class ConfigurationReader
{
    private readonly string _connectionString;
    private SQLiteConnection _connection;
    private IDatabase _redis;
    private ConnectionMultiplexer _multiplexer;
    private readonly string _applicationName;
    private readonly int _refreshTimerIntervalInMs;
    private List<KeyValuePair<string, object>> _configuration;

    public ConfigurationReader(string connectionString, string applicationName, int refreshTimerIntervalInMs)
    {
        _configuration = new List<KeyValuePair<string, object>>();
        _connectionString = connectionString;
        _applicationName = applicationName;
        _refreshTimerIntervalInMs = refreshTimerIntervalInMs;
        _connection = new SQLiteConnection(_connectionString);
        _multiplexer = ConnectionMultiplexer.Connect("localhost");
        InitConfiguration();
    }

    private void InitConfiguration()
    {
        try
        {
            _connection.Open();
            _redis = _multiplexer.GetDatabase();
            GetDataFromDb();
            LoopService();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void GetDataFromDb()
    {
        var sql = $@"SELECT Name,Type,Value FROM Records WHERE IsActive = 1 AND ApplicationName = '{_applicationName}'";

        _configuration.Clear();

        using (var command = _connection.CreateCommand())
        {
            command.CommandText = sql;
            var rs = command.ExecuteReader();
            while (await rs.ReadAsync())
            {
                var name = rs.GetString(0);
                var type = rs.GetString(1);
                Object value = null;
                switch (type)
                {
                    case "String":
                        value = rs.GetString(2);
                        break;
                    case "Int":
                        value = int.Parse(rs.GetString(2));
                        break;
                    case "Boolean":
                        value = int.Parse(rs.GetString(2)) == 1;
                        break;
                    case "Double":
                        value = double.Parse(rs.GetString(2));
                        break;
                    default:
                        value = rs.GetString(2);
                        break;
                }

                _redis.StringSet($"{_applicationName}_{name}", value.ToString(), TimeSpan.FromMilliseconds(_refreshTimerIntervalInMs));
                _configuration.Add(new KeyValuePair<string, object>(name, value));
            }
            _connection.Close();
        }
    }

    private void LoopService()
    {
        Thread thread = new Thread(new ThreadStart(RunSync));
        thread.Start();
    }

    private void RunSync()
    {
        while (true)
        {
            Thread.Sleep(_refreshTimerIntervalInMs);
            if (_connection?.State == ConnectionState.Closed)
            {
                _connection = new SQLiteConnection(_connectionString);
                _connection.Open();
            }

            GetDataFromDb();
        }
    }

    public T GetValue<T>(string key)
    {
        var redisItem = _redis.StringGet($"{_applicationName}_{key}");
        if (redisItem.HasValue)
        {
            object? val;
            if (redisItem == "False" || redisItem == "True")
            {
                val = redisItem == "True";
            }
            else
            {
                val = Convert.ChangeType(redisItem, typeof(T));
            }
            return (T) val;
        }

        return (T) _configuration.Where(x => x.Key == key).Select(x => x.Value).FirstOrDefault();
    }
}