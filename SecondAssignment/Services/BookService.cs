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
}