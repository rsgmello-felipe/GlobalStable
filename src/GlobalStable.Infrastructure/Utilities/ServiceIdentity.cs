namespace GlobalStable.Infrastructure.Utilities;

public class ServiceIdentity
{
    public static readonly string ServiceName = "Bloquo.GlobalStable";
    public static readonly string InstanceName = Environment.MachineName ?? $"unknown_machine_{Guid.NewGuid()}";
}