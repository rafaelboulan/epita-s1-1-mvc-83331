using Library.Domain;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests;

public class BookSearchTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task BookSearchByTitleReturnsExpectedMatches()
    {
        // Arrange
        var context = GetInMemoryContext();
        
        context.Books.AddRange(
            new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Isbn = "1", Category = "Fiction", IsAvailable = true },
            new Book { Title = "1984", Author = "George Orwell", Isbn = "2", Category = "Fiction", IsAvailable = true },
            new Book { Title = "To Kill a Mockingbird", Author = "Harper Lee", Isbn = "3", Category = "Fiction", IsAvailable = true }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await context.Books
            .Where(b => b.Title.Contains("Great"))
            .ToListAsync();

        // Assert
        Assert.Single(results);
        Assert.Equal("The Great Gatsby", results.First().Title);
    }

    [Fact]
    public async Task BookSearchByAuthorReturnsExpectedMatches()
    {
        // Arrange
        var context = GetInMemoryContext();
        
        context.Books.AddRange(
            new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Isbn = "1", Category = "Fiction", IsAvailable = true },
            new Book { Title = "Tender is the Night", Author = "F. Scott Fitzgerald", Isbn = "2", Category = "Fiction", IsAvailable = true },
            new Book { Title = "1984", Author = "George Orwell", Isbn = "3", Category = "Fiction", IsAvailable = true }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await context.Books
            .Where(b => b.Author.Contains("Fitzgerald"))
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task BookFilterByCategoryReturnsExpectedMatches()
    {
        // Arrange
        var context = GetInMemoryContext();
        
        context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", Isbn = "1", Category = "Fiction", IsAvailable = true },
            new Book { Title = "Book 2", Author = "Author 2", Isbn = "2", Category = "Science", IsAvailable = true },
            new Book { Title = "Book 3", Author = "Author 3", Isbn = "3", Category = "Fiction", IsAvailable = true }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await context.Books
            .Where(b => b.Category == "Fiction")
            .ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, b => Assert.Equal("Fiction", b.Category));
    }
}
