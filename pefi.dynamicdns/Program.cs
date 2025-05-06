// See https://aka.ms/new-console-template for more information
using dnsimple;
Console.WriteLine($"{DateTime.UtcNow}: PeFi Dynamic DNS Started");


var client = new Client();
client.AddCredentials(new OAuth2Credentials("dnsimple_a_D8HUCvbQYCorXSAa1ebJuMtWUeZGR8K3"));

IPAddressInfo? previousIPAddress = null;

while (true)
{
    var currentIPAddress = await IpAddressLookup.GetPublicIpAddress();

    if(previousIPAddress != currentIPAddress)
    {
        Console.WriteLine($"{DateTime.UtcNow}: IP Address - Changed -   {{ 'PreviousIpAddress': '{previousIPAddress?.Ip ?? "NOT SET"}',  'CurrentIpAddress': '{currentIPAddress.Ip}' }}");
        await DNSUpdater.UpdateDNSRecord("pefi.co.uk", "home", currentIPAddress);
        previousIPAddress = currentIPAddress;
    }
    else
    {
        Console.WriteLine($"{DateTime.UtcNow}: IP Address - No Change - {{ 'CurrentIpAddress': '{currentIPAddress.Ip}' }}");
    }

    // Wait for 5 minutes before checking again
    await Task.Delay(TimeSpan.FromMinutes(5));
}

