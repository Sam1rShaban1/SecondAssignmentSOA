using SecondAssignment.Models;

namespace SecondAssignment;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooksAsync(int page = 1, int pageSize = 10);
    Task<Book?> GetBookByIdAsync(int id);
}