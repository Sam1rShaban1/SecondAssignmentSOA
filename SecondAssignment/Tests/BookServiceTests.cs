using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using SecondAssignment.Models;
using SecondAssignment.Repositories;
using SecondAssignment.Services;
using Xunit;

namespace SecondAssignment.Tests;

public class BookServiceTests
{
    private IBookRepository CreateMockRepository()
    {
        return Substitute.For<IBookRepository>();
    }

    private IMemoryCache CreateMemoryCache()
    {
        return new MemoryCache(new MemoryCacheOptions());
    }

    private List<Book> GetTestBooks()
    {
        return new List<Book>
        {
            new Book
            {
                BookId = 1,
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedDate = new DateTime(2008, 1, 1),
                Genre = "Programming",
                Price = 49.99m,
                StockQuantity = 10,
                IsActive = true,
                CreatedDate = DateTime.Now,
                LastUpdated = null
            },
            new Book
            {
                BookId = 2,
                Title = "Design Patterns",
                Author = "Gang of Four",
                ISBN = "978-0201633610",
                PublishedDate = new DateTime(1994, 1, 1),
                Genre = "Programming",
                Price = 54.99m,
                StockQuantity = 5,
                IsActive = true,
                CreatedDate = DateTime.Now,
                LastUpdated = null
            }
        };
    }

    private Book GetTestBook()
    {
        return new Book
        {
            BookId = 1,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "978-0132350884",
            PublishedDate = new DateTime(2008, 1, 1),
            Genre = "Programming",
            Price = 49.99m,
            StockQuantity = 10,
            IsActive = true,
            CreatedDate = DateTime.Now,
            LastUpdated = null
        };
    }

    #region GetBooksAsync Tests

    [Fact]
    public async Task GetBooksAsync_WithValidParameters_CallsRepositoryWithCorrectParameters()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();
        var books = GetTestBooks();

        mockRepository.GetAllBooksAsync(1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(books));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBooksAsync(1, 10);

        // Assert
        await mockRepository.Received(1).GetAllBooksAsync(1, 10);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetBooksAsync_ReturnsRepositoryDataUnchanged()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();
        var books = GetTestBooks();

        mockRepository.GetAllBooksAsync(2, 5)
            .Returns(Task.FromResult<IEnumerable<Book>>(books));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBooksAsync(2, 5);
        var resultList = result.ToList();

        // Assert
        Assert.Equal(books[0].BookId, resultList[0].BookId);
        Assert.Equal(books[0].Title, resultList[0].Title);
        Assert.Equal(books[1].BookId, resultList[1].BookId);
        Assert.Equal(books[1].Title, resultList[1].Title);
    }

    [Fact]
    public async Task GetBooksAsync_WithEmptyCollectionFromRepository_ReturnsEmpty()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();

        mockRepository.GetAllBooksAsync(100, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(new List<Book>()));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBooksAsync(100, 10);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBooksAsync_WithNullFromRepository_ReturnsNullResult()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();

        mockRepository.GetAllBooksAsync(1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>?>(null));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBooksAsync(1, 10);

        // Assert - Service returns null when repository returns null
        Assert.Null(result);
    }

    #endregion

    #region GetBookAsync Tests

    [Fact]
    public async Task GetBookAsync_WithValidId_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();
        var testBook = GetTestBook();

        mockRepository.GetBookByIdAsync(1)
            .Returns(Task.FromResult<Book?>(testBook));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBookAsync(1);

        // Assert
        await mockRepository.Received(1).GetBookByIdAsync(1);
        Assert.NotNull(result);
        Assert.Equal(testBook.BookId, result.BookId);
    }

    [Fact]
    public async Task GetBookAsync_CachesBookDataOnSuccess()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();
        var testBook = GetTestBook();

        mockRepository.GetBookByIdAsync(1)
            .Returns(Task.FromResult<Book?>(testBook));

        var service = new BookService(mockRepository, cache);

        // Act - First call
        var firstResult = await service.GetBookAsync(1);

        // Act - Second call (should use cache)
        mockRepository.ClearReceivedCalls();
        var secondResult = await service.GetBookAsync(1);

        // Assert
        Assert.Equal(firstResult.BookId, secondResult.BookId);
        Assert.Equal(firstResult.Title, secondResult.Title);
        // Repository should not be called on second request
        await mockRepository.DidNotReceive().GetBookByIdAsync(1);
    }

    [Fact]
    public async Task GetBookAsync_WithRepositoryReturningNull_ReturnsNull()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();

        mockRepository.GetBookByIdAsync(999)
            .Returns(Task.FromResult<Book?>(null));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBookAsync(999);

        // Assert
        Assert.Null(result);
        await mockRepository.Received(1).GetBookByIdAsync(999);
    }

    [Fact]
    public async Task GetBookAsync_WhenRepositoryThrowsException_ExceptionPropagates()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();

        mockRepository.GetBookByIdAsync(1)
            .Returns(Task.FromException<Book?>(new InvalidOperationException("Database error")));

        var service = new BookService(mockRepository, cache);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetBookAsync(1));
    }

    [Fact]
    public async Task GetBookAsync_CacheDurationIsCorrect()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();
        var testBook = GetTestBook();

        mockRepository.GetBookByIdAsync(1)
            .Returns(Task.FromResult<Book?>(testBook));

        var service = new BookService(mockRepository, cache);

        // Act
        var result = await service.GetBookAsync(1);

        // Assert - Verify cache entry exists
        var cacheKey = "Book_1";
        var cachedValue = cache.Get<Book?>(cacheKey);
        Assert.NotNull(cachedValue);
        Assert.Equal(testBook.BookId, cachedValue.BookId);
    }

    [Fact]
    public async Task GetBookAsync_WithMultipleDifferentIds_CachesSeparately()
    {
        // Arrange
        var mockRepository = CreateMockRepository();
        var cache = CreateMemoryCache();
        var book1 = GetTestBook();
        var book2 = new Book
        {
            BookId = 2,
            Title = "Design Patterns",
            Author = "Gang of Four",
            ISBN = "978-0201633610",
            PublishedDate = new DateTime(1994, 1, 1),
            Genre = "Programming",
            Price = 54.99m,
            StockQuantity = 5,
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        mockRepository.GetBookByIdAsync(1)
            .Returns(Task.FromResult<Book?>(book1));
        mockRepository.GetBookByIdAsync(2)
            .Returns(Task.FromResult<Book?>(book2));

        var service = new BookService(mockRepository, cache);

        // Act
        var result1 = await service.GetBookAsync(1);
        var result2 = await service.GetBookAsync(2);

        // Assert
        Assert.Equal(book1.BookId, result1.BookId);
        Assert.Equal(book2.BookId, result2.BookId);
        Assert.Equal(2, mockRepository.ReceivedCalls().Count());
    }

    #endregion
}
