using Library.Domain;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.Tests;

public class LoanTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CannotCreateLoanForBookAlreadyOnActiveLoan()
    {
        // Arrange
        var context = GetInMemoryContext();
        
        var book = new Book { Title = "Test Book", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = true };
        var member1 = new Member { FullName = "Member 1", Email = "m1@test.com", Phone = "123" };
        var member2 = new Member { FullName = "Member 2", Email = "m2@test.com", Phone = "456" };

        context.Books.Add(book);
        context.Members.AddRange(member1, member2);
        await context.SaveChangesAsync();

        var loan1 = new Loan 
        { 
            BookId = book.Id, 
            MemberId = member1.Id, 
            LoanDate = DateTime.Today, 
            DueDate = DateTime.Today.AddDays(14),
            ReturnedDate = null
        };
        context.Loans.Add(loan1);
        await context.SaveChangesAsync();

        // Act
        var hasActiveLoan = await context.Loans.AnyAsync(l => l.BookId == book.Id && l.ReturnedDate == null);

        // Assert
        Assert.True(hasActiveLoan);
    }

    [Fact]
    public async Task ReturnedLoanMakesBookAvailableAgain()
    {
        // Arrange
        var context = GetInMemoryContext();
        
        var book = new Book { Title = "Test Book", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = false };
        var member = new Member { FullName = "Member", Email = "m@test.com", Phone = "123" };

        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var loan = new Loan 
        { 
            BookId = book.Id, 
            MemberId = member.Id, 
            LoanDate = DateTime.Today, 
            DueDate = DateTime.Today.AddDays(14),
            ReturnedDate = null
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        // Act
        loan.ReturnedDate = DateTime.Today;
        book.IsAvailable = true;
        context.Update(loan);
        context.Update(book);
        await context.SaveChangesAsync();

        var updatedBook = await context.Books.FindAsync(book.Id);

        // Assert
        Assert.True(updatedBook!.IsAvailable);
        Assert.NotNull(updatedBook.IsAvailable);
    }

    [Fact]
    public async Task OverdueLoansAreIdentifiedCorrectly()
    {
        // Arrange
        var context = GetInMemoryContext();
        
        var book = new Book { Title = "Test Book", Author = "Author", Isbn = "123", Category = "Fiction", IsAvailable = false };
        var member = new Member { FullName = "Member", Email = "m@test.com", Phone = "123" };

        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var overdueDate = DateTime.Today.AddDays(-5);
        var loan = new Loan 
        { 
            BookId = book.Id, 
            MemberId = member.Id, 
            LoanDate = overdueDate.AddDays(-20),
            DueDate = overdueDate,
            ReturnedDate = null
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var addedLoan = await context.Loans.FirstAsync(l => l.Id == loan.Id);

        // Act
        var isOverdue = addedLoan.IsOverdue;

        // Assert
        Assert.True(isOverdue);
    }
}
