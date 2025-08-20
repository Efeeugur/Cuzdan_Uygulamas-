using System;

namespace LibraryManagementSystem.Models
{
    public enum BookStatus
    {
        Available,
        Borrowed
    }

    public class Book
    {
        public string BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int PublicationYear { get; set; }
        public BookStatus Status { get; set; }
        public string ISBN { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdated { get; set; }

        public Book()
        {
            BookId = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            LastUpdated = DateTime.Now;
            Status = BookStatus.Available;
        }

        public Book(string title, string author, int publicationYear, string isbn = "") : this()
        {
            Title = title;
            Author = author;
            PublicationYear = publicationYear;
            ISBN = isbn;
        }
    }
}