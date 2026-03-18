using Library.Domain;

namespace Library.MVC.ViewModels;

public class BookIndexViewModel
{
    public List<Book> Books { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public string SelectedCategory { get; set; } = string.Empty;
    public string SelectedAvailability { get; set; } = "All";
    public string SortBy { get; set; } = "Title";
}
