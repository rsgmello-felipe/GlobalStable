using GlobalStable.Domain.Entities;
using GlobalStable.Domain.Interfaces.Repositories;
using GlobalStable.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GlobalStable.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ServiceDbContext _context;

        public CustomerRepository(ServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Customer> AddAsync(Customer entity)
        {
            await _context.Customers.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Customer> GetByIdAsync(long id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Customer> GetByDocumentAsync(string document)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x => x.Document == document);
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer> UpdateAsync(Customer entity)
        {
            _context.Customers.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task RemoveAsync(Customer entity)
        {
            _context.Customers.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}