using Library.Domain;
using Library.MVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers;

public class LoansController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoansController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .ToListAsync();
        return View(loans);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["BookId"] = new SelectList(
            await _context.Books.Where(b => b.IsAvailable).ToListAsync(),
            "Id", "Title");
        ViewData["MemberId"] = new SelectList(
            await _context.Members.ToListAsync(),
            "Id", "FullName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Loan loan)
    {
        // Check if book is available
        var book = await _context.Books.FindAsync(loan.BookId);
        if (book == null || !book.IsAvailable)
        {
            ModelState.AddModelError("BookId", "This book is not available for lending.");
        }

        // Check if book already has an active loan
        var hasActiveLoan = await _context.Loans
            .AnyAsync(l => l.BookId == loan.BookId && l.ReturnedDate == null);
        if (hasActiveLoan)
        {
            ModelState.AddModelError("BookId", "This book is already on an active loan.");
        }

        if (ModelState.IsValid)
        {
            loan.LoanDate = DateTime.Today;
            loan.DueDate = DateTime.Today.AddDays(14);
            _context.Add(loan);
            
            if (book != null)
            {
                book.IsAvailable = false;
                _context.Update(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["BookId"] = new SelectList(
            await _context.Books.Where(b => b.IsAvailable).ToListAsync(),
            "Id", "Title", loan.BookId);
        ViewData["MemberId"] = new SelectList(
            await _context.Members.ToListAsync(),
            "Id", "FullName", loan.MemberId);
        return View(loan);
    }

    public async Task<IActionResult> MarkReturned(int? id)
    {
        if (id == null) return NotFound();

        var loan = await _context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (loan == null) return NotFound();

        loan.ReturnedDate = DateTime.Today;
        _context.Update(loan);

        // Make book available again
        if (loan.Book != null)
        {
            loan.Book.IsAvailable = true;
            _context.Update(loan.Book);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (loan == null) return NotFound();

        return View(loan);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var loan = await _context.Loans.Include(l => l.Book).FirstOrDefaultAsync(l => l.Id == id);
        if (loan != null)
        {
            if (loan.ReturnedDate == null && loan.Book != null)
            {
                loan.Book.IsAvailable = true;
                _context.Update(loan.Book);
            }
            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
