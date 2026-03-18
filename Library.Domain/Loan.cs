namespace Library.Domain;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int MemberId { get; set; }
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; }

    // Navigation properties
    public Book Book { get; set; } = null!;
    public Member Member { get; set; } = null!;

    public bool IsOverdue => ReturnedDate == null && DueDate < DateTime.Today;
    public bool IsActive => ReturnedDate == null;
}
