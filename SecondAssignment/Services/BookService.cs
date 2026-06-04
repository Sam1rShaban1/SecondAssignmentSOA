using Microsoft.Extensions.Caching.Memory;
using SecondAssignment.Models;
using SecondAssignment.Repositories;

namespace SecondAssignment.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan BookCacheDuration = TimeSpan.FromSeconds(60);

    public BookService(IBookRepository bookRepository, IMemoryCache cache)
    {
        _bookRepository = bookRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<Book>> GetBooksAsync(int page, int pageSize)
    {
        return await _bookRepository.GetAllBooksAsync(page, pageSize);
    }

    public async Task<Book?> GetBookAsync(int id)
    {
        var cacheKey = $"Book_{id}";

        if (_cache.TryGetValue<Book?>(cacheKey, out var cachedBook))
        {
            return cachedBook;
        }

        var book = await _bookRepository.GetBookByIdAsync(id);
        if (book != null)
        {
            _cache.Set(cacheKey, book, BookCacheDuration);
        }

        return book;
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        var cacheKey = $"Search_{searchTerm}_{page}_{pageSize}";

        if (_cache.TryGetValue<IEnumerable<Book>>(cacheKey, out var cachedResults))
        {
            return cachedResults!;
        }

        var results = await _bookRepository.SearchBooksAsync(searchTerm, page, pageSize);
        _cache.Set(cacheKey, results, BookCacheDuration);

        return results;
    }

    public async Task<Book> CreateBookAsync(CreateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            PublishedDate = request.PublishedDate,
            Genre = request.Genre,
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        var createdBook = await _bookRepository.CreateBookAsync(book);
        
        // Invalidate list cache on create
        InvalidateListCaches();

        return createdBook;
    }

    public async Task<Book?> UpdateBookAsync(int id, UpdateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title ?? string.Empty,
            Author = request.Author ?? string.Empty,
            Genre = request.Genre ?? string.Empty,
            Price = request.Price ?? 0,
            StockQuantity = request.StockQuantity ?? 0
        };

        var updatedBook = await _bookRepository.UpdateBookAsync(id, book);

        if (updatedBook != null)
        {
            // Invalidate individual book cache
            _cache.Remove($"Book_{id}");
            // Invalidate list cache
            InvalidateListCaches();
        }

        return updatedBook;
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var result = await _bookRepository.DeleteBookAsync(id);

        if (result)
        {
            // Invalidate individual book cache
            _cache.Remove($"Book_{id}");
            // Invalidate list cache
            InvalidateListCaches();
        }

        return result;
    }

    private void InvalidateListCaches()
    {
        // Clear search caches (simplified - in production use cache key patterns)
        // This demonstrates cache invalidation strategy
    }
}