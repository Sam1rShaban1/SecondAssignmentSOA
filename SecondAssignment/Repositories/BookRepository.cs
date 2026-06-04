using Microsoft.EntityFrameworkCore;
using SecondAssignment.Data;
using SecondAssignment.Models;

namespace SecondAssignment.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync(int page = 1, int pageSize = 10)
    {
        return await _context.Books
            .Where(b => b.IsActive)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }
    

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books
            .Where(b => b.BookId == id && b.IsActive)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
    
}