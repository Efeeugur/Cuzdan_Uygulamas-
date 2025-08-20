using System;

namespace LibraryManagementSystem.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class BorrowingRequest
    {
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string BookId { get; set; }
        public DateTime RequestDate { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime? AdminResponseDate { get; set; }
        public string AdminId { get; set; }

        public BorrowingRequest()
        {
            RequestId = Guid.NewGuid().ToString();
            RequestDate = DateTime.Now;
            Status = RequestStatus.Pending;
        }

        public BorrowingRequest(string userId, string bookId) : this()
        {
            UserId = userId;
            BookId = bookId;
        }
    }
}