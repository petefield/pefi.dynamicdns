// See https://aka.ms/new-console-template for more information
using dnsimple;
Console.WriteLine($"{DateTime.UtcNow}: PeFi.Dynamic.DNS Started");


var client = new Client();

var token = Environment.GetEnvironmentVariable("dnsimple_token");

client.AddCredentials(new OAuth2Credentials(token));

IPAddressInfo? previousIPAddress = null;

while (true)
{
    var currentIPAddress = await IpAddressLookup.GetPublicIpAddress();

    if(previousIPAddress != currentIPAddress)
    {
        Console.WriteLine($"{DateTime.UtcNow}: IP Address - Changed - {{ 'PreviousIpAddress': '{previousIPAddress?.Ip ?? "NOT SET"}',  'CurrentIpAddress': '{currentIPAddress.Ip}' }}");
        DNSUpdater.UpdateDNSRecord("pefi.co.uk", "home", currentIPAddress);
        previousIPAddress = currentIPAddress;
    }
    else
    {
        Console.WriteLine($"{DateTime.UtcNow}: IP Address - No Change - {{ 'CurrentIpAddress': '{currentIPAddress.Ip}' }}");
    }

    // Wait for 5 minutes before checking again
    await Task.Delay(TimeSpan.FromMinutes(5));
}

