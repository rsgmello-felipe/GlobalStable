using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByTaxIdAsync(
        string taxId,
        string country);
}