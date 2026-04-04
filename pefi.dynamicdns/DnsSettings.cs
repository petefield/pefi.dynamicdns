namespace pefi.dynamicdns;

public class DnsSettings
{
    public string Domain { get; set; } = "";
    public string HomeHostname { get; set; } = "";
    public string ApiToken { get; set; } = "";
    public int CheckIntervalMinutes { get; set; } = 2;
}
