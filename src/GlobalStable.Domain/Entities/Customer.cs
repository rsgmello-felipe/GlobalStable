using System.ComponentModel.DataAnnotations;

namespace GlobalStable.Domain.Entities;

public class Customer : EntityBase
{
    public string Name { get; private set; }

    public string TaxId { get; private set; }

    public string Country { get; private set; }

    public bool Enabled { get; private set; }

    public Customer(
        string name,
        string taxId,
        string country,
        bool enabled = true)
    {
        Name = name;
        TaxId = taxId;
        Country = country;
        Enabled = enabled;
    }

    public void Update(
        bool? enabled = null)
    {
        Enabled = enabled ?? Enabled;
    }
}