using SecondAssignment.Models;

namespace SecondAssignment;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooksAsync(int page = 1, int pageSize = 10);
    Task<Book?> GetBookByIdAsync(int id);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, int page = 1, int pageSize = 10);
    Task<Book> CreateBookAsync(Book book);
    Task<Book?> UpdateBookAsync(int id, Book book);
    Task<bool> DeleteBookAsync(int id);
}