using LibraryManagementSystem.Models;
using System.Collections.Generic;

namespace LibraryManagementSystem.BusinessLogic.Interfaces
{
    public interface IBookCRUD
    {
        bool AddBook(string title, string author, int publicationYear, string isbn = "");
        bool UpdateBook(string isbn, Book bookData);
        bool DeleteBook(string isbn);
        Book GetBookByIsbn(string isbn);
        List<Book> GetAllBooks();
        List<Book> SearchBooks(string searchTerm);
    }
}