using System.Threading.Tasks;
using GlobalStable.Domain.Entities;

namespace GlobalStable.Domain.Interfaces.Repositories
{
    public interface IOrderStatusRepository : IRepository<OrderStatus>
    {
        Task<OrderStatus?> GetByIdAsync(long orderStatusId);

        Task<long?> GetStatusIdByNameAsync(string statusName);

        Task<OrderStatus?> GetByNameAsync(string name);

        Task<IEnumerable<OrderStatus>> GetAllAsync();

        Task<Dictionary<long, string>> GetAllAsDictionaryAsync();
    }
}