using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetByStatus(EstadoTarefa status);

        T? GetById(int id);
        void Add(T entity);
        void Delete(int id);
        void Update(T entity);
    }
}
