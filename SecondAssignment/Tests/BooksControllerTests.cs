using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SecondAssignment.Controllers;
using SecondAssignment.Models;
using SecondAssignment.Services;
using Xunit;

namespace SecondAssignment.Tests;

public class BooksControllerTests
{
    private IBookService CreateMockService()
    {
        return Substitute.For<IBookService>();
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

    #region GetBooks Endpoint Tests

    [Fact]
    public async Task GetBooks_WithValidParameters_Returns200OkWithData()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBooks = GetTestBooks();

        mockService.GetBooksAsync(1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(testBooks));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks(page: 1, pageSize: 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.NotNull(okResult.Value);
        Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
    }

    [Fact]
    public async Task GetBooks_WithValidPageAndPageSize_AcceptsParameters()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBooks = GetTestBooks();

        mockService.GetBooksAsync(2, 5)
            .Returns(Task.FromResult<IEnumerable<Book>>(testBooks));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks(page: 2, pageSize: 5);

        // Assert
        await mockService.Received(1).GetBooksAsync(2, 5);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WithPageZero_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks(page: 0, pageSize: 10);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetBooks_WithNegativePage_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks(page: -1, pageSize: 10);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WithPageSizeZero_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks(page: 1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WithNegativePageSize_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks(page: 1, pageSize: -5);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetBooks_WithDefaultParameters_UsesDefaultValues()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBooks = GetTestBooks();

        mockService.GetBooksAsync(1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(testBooks));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks();

        // Assert
        await mockService.Received(1).GetBooksAsync(1, 10);
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetBooks_ReturnsEmptyListWhenNoBooks_Returns200Ok()
    {
        // Arrange
        var mockService = CreateMockService();

        mockService.GetBooksAsync(1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(new List<Book>()));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBooks();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var books = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
        Assert.Empty(books);
    }

    #endregion

    #region GetBook Endpoint Tests

    [Fact]
    public async Task GetBook_WithValidId_Returns200OkWithBookData()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBook = GetTestBook();

        mockService.GetBookAsync(1)
            .Returns(Task.FromResult<Book?>(testBook));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var returnedBook = Assert.IsType<Book>(okResult.Value);
        Assert.Equal(testBook.BookId, returnedBook.BookId);
        Assert.Equal(testBook.Title, returnedBook.Title);
    }

    [Fact]
    public async Task GetBook_WithValidId_CallsServiceWithCorrectId()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBook = GetTestBook();

        mockService.GetBookAsync(5)
            .Returns(Task.FromResult<Book?>(testBook));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: 5);

        // Assert
        await mockService.Received(1).GetBookAsync(5);
    }

    [Fact]
    public async Task GetBook_WithInvalidIdZero_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetBook_WithNegativeId_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: -1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetBook_WithNonExistentId_Returns404NotFound()
    {
        // Arrange
        var mockService = CreateMockService();

        mockService.GetBookAsync(999)
            .Returns(Task.FromResult<Book?>(null));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: 999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetBook_WithServiceReturningNull_Returns404NotFound()
    {
        // Arrange
        var mockService = CreateMockService();

        mockService.GetBookAsync(1)
            .Returns(Task.FromResult<Book?>(null));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: 1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Contains("not found", (string)notFoundResult.Value!);
    }

    [Fact]
    public async Task GetBook_WithLargeValidId_Returns200OkOrNotFound()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBook = GetTestBook();

        mockService.GetBookAsync(int.MaxValue)
            .Returns(Task.FromResult<Book?>(null));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.GetBook(id: int.MaxValue);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetBook_WhenServiceThrowsException_ExceptionPropagates()
    {
        // Arrange
        var mockService = CreateMockService();

        mockService.GetBookAsync(1)
            .Returns(Task.FromException<Book?>(new InvalidOperationException("Database error")));

        var controller = new BooksController(mockService);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.GetBook(1));
    }

    #endregion
}
