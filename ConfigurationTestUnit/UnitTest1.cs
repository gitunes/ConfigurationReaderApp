using ConfigurationApp;

namespace ConfigurationTestUnit;

public class Tests
{
    private ConfigurationReader _configurationReader;
    private string _connectionString;
    private string _applicationName;
    private int _refreshTimerIntervalInMs;

    [SetUp]
    public void Setup()
    {
        _connectionString = @"Data Source=records.db; Version=3;";
        _applicationName = "SERVICE-A";
        _refreshTimerIntervalInMs = 10000;
        _configurationReader = new ConfigurationReader(_connectionString, _applicationName, _refreshTimerIntervalInMs);
    }

    [Test]
    public void TestGetValue()
    {
        Assert.AreEqual(50.5, _configurationReader.GetValue<double>("Price"));
    }
    
    [Test]
    public void Test2GetValue()
    {
        Assert.AreEqual("getir.com", _configurationReader.GetValue<string>("SiteName"));
    }
    
    [Test]
    public void Test3GetValue()
    {
        Assert.AreEqual(true, _configurationReader.GetValue<bool>("IsNew"));
    }
    
}