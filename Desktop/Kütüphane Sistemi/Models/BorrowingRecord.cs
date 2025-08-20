using System;

namespace LibraryManagementSystem.Models
{
    public enum BorrowingStatus
    {
        Active,
        Returned,
        Overdue
    }

    public class BorrowingRecord
    {
        public string RecordId { get; set; }
        public string UserId { get; set; }
        public string BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowingStatus Status { get; set; }

        public BorrowingRecord()
        {
            RecordId = Guid.NewGuid().ToString();
            BorrowDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(14); // 2 weeks borrowing period
            Status = BorrowingStatus.Active;
        }

        public BorrowingRecord(string userId, string bookId) : this()
        {
            UserId = userId;
            BookId = bookId;
        }
    }
}