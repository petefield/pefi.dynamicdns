namespace pefi.dynamicdns.Services;

public interface IIpAddressLookup
{
    Task<IPAddressInfo> GetPublicIpAddress();
}
