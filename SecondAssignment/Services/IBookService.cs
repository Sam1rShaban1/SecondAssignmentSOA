using Microsoft.Extensions.Caching.Memory;
using SecondAssignment.Models;

namespace SecondAssignment.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetBooksAsync(int page, int pageSize);
    Task<Book?> GetBookAsync(int id);
}