namespace SecondAssignment.Models;

public class CreateBookRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string Genre { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

public class UpdateBookRequest
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public decimal? Price { get; set; }
    public int? StockQuantity { get; set; }
}
