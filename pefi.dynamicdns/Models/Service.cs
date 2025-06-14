


public class Service(
    string ServiceName, 
    string? HostName, 
    string? ContainerPortNumber, 
    string? HostPortNumber, 
    string? DockerImageUrl)
{

    public string ServiceName { get; set; } = ServiceName;

    public string? HostName { get; set; } = HostName;

    public string? ContainerPortNumber { get; set; } = ContainerPortNumber;

    public string? HostPortNumber { get; set; } = HostPortNumber;

    public string? DockerImageUrl { get; } = DockerImageUrl;
}
