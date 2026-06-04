using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecondAssignment.Models;
using SecondAssignment.Services;

namespace SecondAssignment.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }
    
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetBooks(int page = 1, int pageSize = 10)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than 0");
            
        var result = await _bookService.GetBooksAsync(page, pageSize);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetBook(int id)
    {
        if (id < 1)
            return BadRequest("Id must be greater than 0");
            
        var book = await _bookService.GetBookAsync(id);
        
        if (book == null)
            return NotFound($"Book with id {id} not found");
            
        return Ok(book);
    }

    [HttpGet("search")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> SearchBooks(string searchTerm, int page = 1, int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term cannot be empty");
            
        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than 0");

        var results = await _bookService.SearchBooksAsync(searchTerm, page, pageSize);
        return Ok(results);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBook([FromBody] CreateBookRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Author))
            return BadRequest("Title and Author are required");

        if (request.Price < 0 || request.StockQuantity < 0)
            return BadRequest("Price and StockQuantity cannot be negative");

        var book = await _bookService.CreateBookAsync(request);
        return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, book);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
    {
        if (id < 1)
            return BadRequest("Id must be greater than 0");

        var updatedBook = await _bookService.UpdateBookAsync(id, request);

        if (updatedBook == null)
            return NotFound($"Book with id {id} not found");

        return Ok(updatedBook);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        if (id < 1)
            return BadRequest("Id must be greater than 0");

        var result = await _bookService.DeleteBookAsync(id);

        if (!result)
            return NotFound($"Book with id {id} not found");

        return NoContent();
    }
}