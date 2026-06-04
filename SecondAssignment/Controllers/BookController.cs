using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}