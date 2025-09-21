namespace GlobalStable.Domain.Entities;

public class CustomerApiKey
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public string KeyHash { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public bool Enabled { get; private set; } = true;

    private CustomerApiKey() {}
    public CustomerApiKey(long customerId, string keyHash)
    {
        CustomerId = customerId;
        KeyHash = keyHash;
        CreatedAt = DateTimeOffset.Now;
        Enabled = true;
    }

    public void Deactivate() => Enabled = false;
}
