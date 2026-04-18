namespace pefi.dynamicdns.Infrastructure;

public interface IDNSClient
{
    Result AddCNAMERecord(string name, string content);
    Result UpdateDNSRecord(string name, IPAddressInfo ipAddressInfo);
    Result DeleteDnsRecord(string name);
}
