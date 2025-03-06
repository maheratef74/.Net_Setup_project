namespace Core.Interfaces.IRepositories;

public interface IGenericRepository<T> where T : class
{
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<List<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task Update(T entity);
    Task Delete(T entity);
}