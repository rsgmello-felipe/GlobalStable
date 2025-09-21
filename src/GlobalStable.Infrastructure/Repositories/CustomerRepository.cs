using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories
{
    public class CustomerRepository(ServiceDbContext context) 
        : Repository<Customer>(context), ICustomerRepository
    {
        public async Task<Customer?> GetByTaxIdAsync(
            string taxId,
            string country)
        {
            return await context.Customers
                .FirstOrDefaultAsync(c => 
                    c.TaxId == taxId && 
                    c.Country == country);
        }
    }
}