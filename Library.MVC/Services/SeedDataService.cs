using Bogus;
using Library.Domain;
using Library.MVC.Data;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Services;

public static class SeedDataService
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.Books.Any() || context.Members.Any() || context.Loans.Any())
            return;

        // Seed Books
        var books = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Author, f => f.Person.FullName)
            .RuleFor(b => b.Isbn, f => f.Random.Replace("###-#-#####-##-#"))
            .RuleFor(b => b.Category, f => f.PickRandom("Fiction", "Science", "History", "Biography", "Technology"))
            .RuleFor(b => b.IsAvailable, true)
            .Generate(20);

        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();

        // Seed Members
        var members = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Person.FullName)
            .RuleFor(m => m.Email, (f, m) => f.Internet.Email(m.FullName))
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber())
            .Generate(10);

        await context.Members.AddRangeAsync(members);
        await context.SaveChangesAsync();

        // Seed Loans (15 loans: some returned, some active, some overdue)
        var booksFromDb = await context.Books.ToListAsync();
        var membersFromDb = await context.Members.ToListAsync();
        var today = DateTime.Today;

        var loans = new List<Loan>();
        var faker = new Faker();

        // Create 15 loans with varied statuses
        for (int i = 0; i < 15; i++)
        {
            var loanDate = today.AddDays(faker.Random.Int(-60, -5));
            var dueDate = loanDate.AddDays(14);
            DateTime? returnedDate = null;

            // Mix of returned, active, and overdue
            var status = faker.PickRandom(new[] { "returned", "active", "overdue" });
            if (status == "returned")
            {
                returnedDate = faker.Date.Between(dueDate, today);
            }
            else if (status == "overdue")
            {
                // Overdue but not returned
                returnedDate = null;
                dueDate = today.AddDays(faker.Random.Int(-30, -1));
            }

            loans.Add(new Loan
            {
                BookId = booksFromDb[i % booksFromDb.Count].Id,
                MemberId = membersFromDb[i % membersFromDb.Count].Id,
                LoanDate = loanDate,
                DueDate = dueDate,
                ReturnedDate = returnedDate
            });
        }

        await context.Loans.AddRangeAsync(loans);

        // Update book availability based on active loans
        var activeLoans = await context.Loans
            .Where(l => l.ReturnedDate == null)
            .Select(l => l.BookId)
            .Distinct()
            .ToListAsync();

        foreach (var book in booksFromDb)
        {
            book.IsAvailable = !activeLoans.Contains(book.Id);
        }

        context.Books.UpdateRange(booksFromDb);
        await context.SaveChangesAsync();
    }
}
