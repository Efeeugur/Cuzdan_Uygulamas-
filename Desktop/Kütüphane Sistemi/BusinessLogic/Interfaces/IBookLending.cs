using LibraryManagementSystem.Models;
using System.Collections.Generic;

namespace LibraryManagementSystem.BusinessLogic.Interfaces
{
    public interface IBookLending
    {
        bool LendBook(string userId, string isbn);
        bool ReturnBook(string userId, string isbn);
        List<Book> GetBorrowedBooks(string userId);
        List<Book> GetAvailableBooks();
        List<BorrowingRecord> GetBorrowingHistory(string userId);
        bool RequestBook(string userId, string isbn);
        List<BorrowingRequest> GetPendingRequests();
        bool ApproveRequest(string requestId, string adminId);
        bool RejectRequest(string requestId, string adminId);
    }
}