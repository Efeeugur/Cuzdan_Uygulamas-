using LibraryManagementSystem.BusinessLogic.Interfaces;
using LibraryManagementSystem.DataAccess;
using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.BusinessLogic.Services
{
    public class BookManager : IBookCRUD, IBookLending
    {
        private readonly FileDataService _dataService;

        public BookManager(FileDataService dataService)
        {
            _dataService = dataService;
        }

        public bool AddBook(string title, string author, int publicationYear, string isbn = "")
        {
            var books = _dataService.LoadBooks();
            var newBook = new Book(title, author, publicationYear, isbn);
            books.Add(newBook);
            _dataService.SaveBooks(books);
            return true;
        }

        public bool UpdateBook(string isbn, Book bookData)
        {
            var books = _dataService.LoadBooks();
            var book = books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null)
                return false;

            book.Title = bookData.Title;
            book.Author = bookData.Author;
            book.PublicationYear = bookData.PublicationYear;
            book.ISBN = bookData.ISBN;
            book.LastUpdated = DateTime.Now;

            _dataService.SaveBooks(books);
            return true;
        }

        public bool DeleteBook(string isbn)
        {
            var books = _dataService.LoadBooks();
            var book = books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null || book.Status == BookStatus.Borrowed)
                return false;

            books.Remove(book);
            _dataService.SaveBooks(books);
            return true;
        }

        public Book GetBookByIsbn(string isbn)
        {
            var books = _dataService.LoadBooks();
            return books.FirstOrDefault(b => b.ISBN == isbn);
        }

        public List<Book> GetAllBooks()
        {
            return _dataService.LoadBooks();
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            var books = _dataService.LoadBooks();
            return books.Where(b => 
                b.Title.ToLower().Contains(searchTerm.ToLower()) ||
                b.Author.ToLower().Contains(searchTerm.ToLower()) ||
                b.ISBN.ToLower().Contains(searchTerm.ToLower())
            ).ToList();
        }

        public bool LendBook(string userId, string isbn)
        {
            var books = _dataService.LoadBooks();
            var book = books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null || book.Status == BookStatus.Borrowed)
                return false;

            book.Status = BookStatus.Borrowed;
            _dataService.SaveBooks(books);

            var borrowingRecords = _dataService.LoadBorrowingRecords();
            var newRecord = new BorrowingRecord(userId, book.BookId);
            borrowingRecords.Add(newRecord);
            _dataService.SaveBorrowingRecords(borrowingRecords);

            return true;
        }

        public bool ReturnBook(string userId, string isbn)
        {
            var books = _dataService.LoadBooks();
            var book = books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null || book.Status == BookStatus.Available)
                return false;

            var borrowingRecords = _dataService.LoadBorrowingRecords();
            var record = borrowingRecords.FirstOrDefault(r => 
                r.UserId == userId && 
                r.BookId == book.BookId && 
                r.Status == BorrowingStatus.Active);

            if (record == null)
                return false;

            book.Status = BookStatus.Available;
            record.ReturnDate = DateTime.Now;
            record.Status = BorrowingStatus.Returned;

            _dataService.SaveBooks(books);
            _dataService.SaveBorrowingRecords(borrowingRecords);

            return true;
        }

        public List<Book> GetBorrowedBooks(string userId)
        {
            var borrowingRecords = _dataService.LoadBorrowingRecords();
            var books = _dataService.LoadBooks();

            var borrowedBookIds = borrowingRecords
                .Where(r => r.UserId == userId && r.Status == BorrowingStatus.Active)
                .Select(r => r.BookId)
                .ToList();

            return books.Where(b => borrowedBookIds.Contains(b.BookId)).ToList();
        }

        public List<Book> GetAvailableBooks()
        {
            var books = _dataService.LoadBooks();
            return books.Where(b => b.Status == BookStatus.Available).ToList();
        }

        public List<BorrowingRecord> GetBorrowingHistory(string userId)
        {
            var borrowingRecords = _dataService.LoadBorrowingRecords();
            return borrowingRecords.Where(r => r.UserId == userId).ToList();
        }

        public bool RequestBook(string userId, string isbn)
        {
            var books = _dataService.LoadBooks();
            var book = books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null || book.Status == BookStatus.Borrowed)
                return false;

            var requests = _dataService.LoadBorrowingRequests();
            
            var existingRequest = requests.FirstOrDefault(r => 
                r.UserId == userId && 
                r.BookId == book.BookId && 
                r.Status == RequestStatus.Pending);

            if (existingRequest != null)
                return false;

            var newRequest = new BorrowingRequest(userId, book.BookId);
            requests.Add(newRequest);
            _dataService.SaveBorrowingRequests(requests);

            return true;
        }

        public List<BorrowingRequest> GetPendingRequests()
        {
            var requests = _dataService.LoadBorrowingRequests();
            return requests.Where(r => r.Status == RequestStatus.Pending).ToList();
        }

        public bool ApproveRequest(string requestId, string adminId)
        {
            var requests = _dataService.LoadBorrowingRequests();
            var request = requests.FirstOrDefault(r => r.RequestId == requestId);

            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Approved;
            request.AdminResponseDate = DateTime.Now;
            request.AdminId = adminId;

            _dataService.SaveBorrowingRequests(requests);

            // Find the book by its ID from the request and get its ISBN
            var books = _dataService.LoadBooks();
            var book = books.FirstOrDefault(b => b.BookId == request.BookId);
            if (book == null) return false;
            
            return LendBook(request.UserId, book.ISBN);
        }

        public bool RejectRequest(string requestId, string adminId)
        {
            var requests = _dataService.LoadBorrowingRequests();
            var request = requests.FirstOrDefault(r => r.RequestId == requestId);

            if (request == null || request.Status != RequestStatus.Pending)
                return false;

            request.Status = RequestStatus.Rejected;
            request.AdminResponseDate = DateTime.Now;
            request.AdminId = adminId;

            _dataService.SaveBorrowingRequests(requests);
            return true;
        }
    }
}