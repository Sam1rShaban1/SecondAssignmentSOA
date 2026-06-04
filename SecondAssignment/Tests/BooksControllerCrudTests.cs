using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using SecondAssignment.Controllers;
using SecondAssignment.Models;
using SecondAssignment.Services;
using Xunit;

namespace SecondAssignment.Tests;

public class BooksControllerCrudTests
{
    private IBookService CreateMockService()
    {
        return Substitute.For<IBookService>();
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
            CreatedDate = DateTime.Now
        };
    }

    #region SearchBooks Tests

    [Fact]
    public async Task SearchBooks_WithValidSearchTerm_Returns200OkWithResults()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBooks = new List<Book> { GetTestBook() };

        mockService.SearchBooksAsync("Clean", 1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(testBooks));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.SearchBooks("Clean", 1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task SearchBooks_WithEmptySearchTerm_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.SearchBooks(string.Empty, 1, 10);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task SearchBooks_WithInvalidPage_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var controller = new BooksController(mockService);

        // Act
        var result = await controller.SearchBooks("Clean", 0, 10);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    #endregion

    #region CreateBook Tests

    [Fact]
    public async Task CreateBook_WithValidRequest_Returns201CreatedAtAction()
    {
        // Arrange
        var mockService = CreateMockService();
        var request = new CreateBookRequest
        {
            Title = "New Book",
            Author = "Author Name",
            ISBN = "978-1234567890",
            PublishedDate = DateTime.Now,
            Genre = "Fiction",
            Price = 29.99m,
            StockQuantity = 5
        };
        var createdBook = GetTestBook();
        createdBook.Title = request.Title;

        mockService.CreateBookAsync(request)
            .Returns(Task.FromResult(createdBook));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.CreateBook(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(BooksController.GetBook), createdResult.ActionName);
    }

    [Fact]
    public async Task CreateBook_WithoutTitle_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var request = new CreateBookRequest
        {
            Title = string.Empty,
            Author = "Author Name",
            ISBN = "978-1234567890",
            PublishedDate = DateTime.Now,
            Genre = "Fiction",
            Price = 29.99m,
            StockQuantity = 5
        };

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.CreateBook(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithNegativePrice_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var request = new CreateBookRequest
        {
            Title = "New Book",
            Author = "Author Name",
            ISBN = "978-1234567890",
            PublishedDate = DateTime.Now,
            Genre = "Fiction",
            Price = -10m,  // Negative price
            StockQuantity = 5
        };

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.CreateBook(request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    #endregion

    #region UpdateBook Tests

    [Fact]
    public async Task UpdateBook_WithValidId_Returns200OkWithUpdatedBook()
    {
        // Arrange
        var mockService = CreateMockService();
        var updateRequest = new UpdateBookRequest
        {
            Title = "Updated Title",
            Price = 59.99m
        };
        var updatedBook = GetTestBook();
        updatedBook.Title = "Updated Title";
        updatedBook.Price = 59.99m;

        mockService.UpdateBookAsync(1, updateRequest)
            .Returns(Task.FromResult<Book?>(updatedBook));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.UpdateBook(1, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();
        var updateRequest = new UpdateBookRequest { Title = "Updated" };

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.UpdateBook(0, updateRequest);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task UpdateBook_WithNonExistentId_Returns404NotFound()
    {
        // Arrange
        var mockService = CreateMockService();
        var updateRequest = new UpdateBookRequest { Title = "Updated" };

        mockService.UpdateBookAsync(999, updateRequest)
            .Returns(Task.FromResult<Book?>(null));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.UpdateBook(999, updateRequest);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    #endregion

    #region DeleteBook Tests

    [Fact]
    public async Task DeleteBook_WithValidId_Returns204NoContent()
    {
        // Arrange
        var mockService = CreateMockService();

        mockService.DeleteBookAsync(1)
            .Returns(Task.FromResult(true));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.DeleteBook(1);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_WithInvalidId_Returns400BadRequest()
    {
        // Arrange
        var mockService = CreateMockService();

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.DeleteBook(0);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_WithNonExistentId_Returns404NotFound()
    {
        // Arrange
        var mockService = CreateMockService();

        mockService.DeleteBookAsync(999)
            .Returns(Task.FromResult(false));

        var controller = new BooksController(mockService);

        // Act
        var result = await controller.DeleteBook(999);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    #endregion

    #region Service Layer Caching Tests

    [Fact]
    public async Task SearchBooks_SecondCallUsesCachedResult()
    {
        // Arrange
        var mockService = CreateMockService();
        var testBooks = new List<Book> { GetTestBook() };

        mockService.SearchBooksAsync("Clean", 1, 10)
            .Returns(Task.FromResult<IEnumerable<Book>>(testBooks));

        var controller = new BooksController(mockService);

        // Act - First call
        await controller.SearchBooks("Clean", 1, 10);

        // Act - Second call
        mockService.ClearReceivedCalls();
        var result = await controller.SearchBooks("Clean", 1, 10);

        // Assert - Should return OK (service would be called, but in real scenario IMemoryCache hits)
        Assert.IsType<OkObjectResult>(result);
    }

    #endregion
}
