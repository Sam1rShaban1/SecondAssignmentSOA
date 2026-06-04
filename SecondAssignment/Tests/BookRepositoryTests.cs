using Microsoft.EntityFrameworkCore;
using SecondAssignment.Data;
using SecondAssignment.Models;
using SecondAssignment.Repositories;
using Xunit;

namespace SecondAssignment.Tests;

public class BookRepositoryTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
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
            },
            new Book
            {
                BookId = 3,
                Title = "Archived Book",
                Author = "Someone",
                ISBN = "978-9999999999",
                PublishedDate = new DateTime(2000, 1, 1),
                Genre = "History",
                Price = 29.99m,
                StockQuantity = 0,
                IsActive = false,
                CreatedDate = DateTime.Now,
                LastUpdated = null
            },
            new Book
            {
                BookId = 4,
                Title = "SOLID Principles",
                Author = "Uncle Bob",
                ISBN = "978-1234567890",
                PublishedDate = new DateTime(2015, 1, 1),
                Genre = "Programming",
                Price = 39.99m,
                StockQuantity = 20,
                IsActive = true,
                CreatedDate = DateTime.Now,
                LastUpdated = null
            },
            new Book
            {
                BookId = 5,
                Title = "Refactoring",
                Author = "Martin Fowler",
                ISBN = "978-0134757599",
                PublishedDate = new DateTime(2018, 1, 1),
                Genre = "Programming",
                Price = 59.99m,
                StockQuantity = 15,
                IsActive = true,
                CreatedDate = DateTime.Now,
                LastUpdated = null
            }
        };
    }

    #region GetAllBooksAsync Tests

    [Fact]
    public async Task GetAllBooksAsync_WithValidPageAndPageSize_ReturnsCorrectBooks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllBooksAsync(page: 1, pageSize: 2);

        // Assert
        Assert.NotNull(result);
        var books = result.ToList();
        Assert.Equal(2, books.Count);
        Assert.Equal("Clean Code", books[0].Title);
        Assert.Equal("Design Patterns", books[1].Title);
    }

    [Fact]
    public async Task GetAllBooksAsync_RespectsPagination_ReturnCorrectPage()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act - Get second page with pageSize of 2
        var result = await repository.GetAllBooksAsync(page: 2, pageSize: 2);

        // Assert
        var books = result.ToList();
        Assert.Equal(2, books.Count);
        Assert.Equal("SOLID Principles", books[0].Title);
        Assert.Equal("Refactoring", books[1].Title);
    }

    [Fact]
    public async Task GetAllBooksAsync_FiltersOnlyActiveBooks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllBooksAsync(page: 1, pageSize: 100);

        // Assert
        var books = result.ToList();
        Assert.Equal(4, books.Count); // Only 4 active books (excluding IsActive=false)
        Assert.All(books, book => Assert.True(book.IsActive));
    }

    [Fact]
    public async Task GetAllBooksAsync_WithInvalidPageZero_ReturnsAllActiveBooks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act - Page 0 with LINQ Skip(-10) doesn't skip, returns first 10
        var result = await repository.GetAllBooksAsync(page: 0, pageSize: 10);

        // Assert - Negative skip is ignored, so all active books are returned
        var books = result.ToList();
        Assert.Equal(4, books.Count); // All active books fit in first 10
    }

    [Fact]
    public async Task GetAllBooksAsync_WithNegativePage_ReturnsAllActiveBooks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act - Negative page with LINQ Skip(-60) doesn't skip, returns first 10
        var result = await repository.GetAllBooksAsync(page: -5, pageSize: 10);

        // Assert - Negative skip is ignored, so all active books are returned
        var books = result.ToList();
        Assert.Equal(4, books.Count); // All active books fit in first 10
    }

    [Fact]
    public async Task GetAllBooksAsync_WithEmptyDatabase_ReturnsEmpty()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllBooksAsync(page: 1, pageSize: 10);

        // Assert
        var books = result.ToList();
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetAllBooksAsync_WithPageNumberBeyondDataRange_ReturnsEmpty()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act - Request page 100 with only 4 active books
        var result = await repository.GetAllBooksAsync(page: 100, pageSize: 10);

        // Assert
        var books = result.ToList();
        Assert.Empty(books);
    }

    [Fact]
    public async Task GetAllBooksAsync_WithDefaultParameters_UsesDefaultValues()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetAllBooksAsync();

        // Assert
        var books = result.ToList();
        Assert.Equal(4, books.Count); // All 4 active books fit in first page with default pageSize=10
    }

    #endregion

    #region GetBookByIdAsync Tests

    [Fact]
    public async Task GetBookByIdAsync_WithValidId_ReturnsCorrectBook()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Clean Code", result.Title);
        Assert.Equal("Robert C. Martin", result.Author);
    }

    [Fact]
    public async Task GetBookByIdAsync_OnlyReturnsActiveBooks()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(3); // This book has IsActive=false

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithNegativeId_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(-1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithZeroId_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var testBooks = GetTestBooks();
        context.Books.AddRange(testBooks);
        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(0);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_WithEmptyDatabase_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetBookByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    #endregion
}
