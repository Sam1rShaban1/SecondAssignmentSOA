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

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        return await _context.Books
            .Where(b => b.IsActive && 
                   (b.Title.Contains(searchTerm) || 
                    b.Author.Contains(searchTerm) || 
                    b.Genre.Contains(searchTerm)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        book.IsActive = true;
        book.CreatedDate = DateTime.UtcNow;
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<Book?> UpdateBookAsync(int id, Book updatedBook)
    {
        var book = await _context.Books
            .Where(b => b.BookId == id && b.IsActive)
            .FirstOrDefaultAsync();

        if (book == null)
            return null;

        if (!string.IsNullOrEmpty(updatedBook.Title))
            book.Title = updatedBook.Title;
        if (!string.IsNullOrEmpty(updatedBook.Author))
            book.Author = updatedBook.Author;
        if (!string.IsNullOrEmpty(updatedBook.Genre))
            book.Genre = updatedBook.Genre;
        if (updatedBook.Price > 0)
            book.Price = updatedBook.Price;
        if (updatedBook.StockQuantity >= 0)
            book.StockQuantity = updatedBook.StockQuantity;

        book.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _context.Books
            .Where(b => b.BookId == id && b.IsActive)
            .FirstOrDefaultAsync();

        if (book == null)
            return false;

        book.IsActive = false;
        book.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}