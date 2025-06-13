// See https://aka.ms/new-console-template for more information
namespace pefi.dynamicdns.Infrastructure
{
    public interface IDNSClient
    {
        void AddCNAMERecord(string domain, string host, string content);
        void UpdateDNSRecord(string domain, string recordName, IPAddressInfo ipAddressInfo);
        void DeleteDnsRecord(string domain);
    }
}