using Microsoft.Extensions.Caching.Memory;
using SecondAssignment.Models;

namespace SecondAssignment.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetBooksAsync(int page, int pageSize);
    Task<Book?> GetBookAsync(int id);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<Book> CreateBookAsync(CreateBookRequest request);
    Task<Book?> UpdateBookAsync(int id, UpdateBookRequest request);
    Task<bool> DeleteBookAsync(int id);
}