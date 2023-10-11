namespace APIintegrationBookShop.Models
{
    public class BorrowRequest
    {
        public int PatronId { get; set; }
        public int BookId { get; set; }
        public int accountNumber { get; set;}
       public decimal withdrawalAmount { get; set;}
        public string BankEmail { get; set;}
        public string BankPassword { get; set;}
    }
}
