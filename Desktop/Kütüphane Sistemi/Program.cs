using LibraryManagementSystem.BusinessLogic.Interfaces;
using LibraryManagementSystem.BusinessLogic.Services;
using LibraryManagementSystem.DataAccess;
using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem
{
    class Program
    {
        private static IUserManager _userManager;
        private static IBookCRUD _bookCRUD;
        private static IBookLending _bookLending;
        private static User _currentUser;

        static void Main(string[] args)
        {
            try
            {
                var dataService = new FileDataService();

                _userManager = new UserManager(dataService);
                var bookManager = new BookManager(dataService);
                _bookCRUD = bookManager;
                _bookLending = bookManager;

                // Debug: Check if books are loaded
                var books = _bookCRUD.GetAllBooks();
                Console.WriteLine($"DEBUG: {books.Count} books loaded from JSON files.");
                if (books.Count > 0)
                {
                    Console.WriteLine($"DEBUG: First book: {books[0].Title}");
                }

                Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void Start()
        {
            Console.WriteLine("Library Management System");
            Console.WriteLine("=============================");

            while (true)
            {
                if (_currentUser == null)
                {
                    ShowLoginMenu();
                }
                else
                {
                    if (_currentUser.Role == UserRole.Admin)
                    {
                        ShowAdminMenu();
                    }
                    else
                    {
                        ShowUserMenu();
                    }
                }
            }
        }

        private static void ShowLoginMenu()
        {
            Console.WriteLine("\n--- Login Menu ---");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register New User");
            Console.WriteLine("3. Exit");
            Console.Write("Choose an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    RegisterUser();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        private static void Login()
        {
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = ReadPassword();

            _currentUser = _userManager.AuthenticateUser(username, password);

            if (_currentUser == null)
            {
                Console.WriteLine("\nInvalid credentials. Please try again.");
            }
            else
            {
                Console.WriteLine($"\nWelcome, {_currentUser.Username}!");
                if (_currentUser.Role == UserRole.Admin)
                {
                    ShowPendingRequests();
                }
            }
        }

        private static void RegisterUser()
        {
            Console.Write("Username: ");
            var username = Console.ReadLine();
            Console.Write("Password: ");
            var password = ReadPassword();
            Console.WriteLine("\nSelect Role:");
            Console.WriteLine("1. Regular User");
            Console.WriteLine("2. Admin");
            Console.Write("Choice: ");
            var roleChoice = Console.ReadLine();

            var role = roleChoice == "2" ? UserRole.Admin : UserRole.RegularUser;

            if (_userManager.CreateUser(username, password, role))
            {
                Console.WriteLine("\nUser registered successfully!");
            }
            else
            {
                Console.WriteLine("\nRegistration failed. Username might already exist.");
            }
        }

        private static void ShowAdminMenu()
        {
            Console.WriteLine($"\n--- Admin Menu ({_currentUser.Username}) ---");
            Console.WriteLine("1. Add New Book");
            Console.WriteLine("2. List All Books");
            Console.WriteLine("3. Update Book");
            Console.WriteLine("4. Delete Book");
            Console.WriteLine("5. Lend Book to User");
            Console.WriteLine("6. Process Book Return");
            Console.WriteLine("7. View Pending Requests");
            Console.WriteLine("8. Search Books");
            Console.WriteLine("9. Logout");
            Console.Write("Choose an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddNewBook();
                    break;
                case "2":
                    ListAllBooks();
                    break;
                case "3":
                    UpdateBook();
                    break;
                case "4":
                    DeleteBook();
                    break;
                case "5":
                    LendBookToUser();
                    break;
                case "6":
                    ProcessBookReturn();
                    break;
                case "7":
                    ViewPendingRequests();
                    break;
                case "8":
                    SearchBooks();
                    break;
                case "9":
                    _currentUser = null;
                    Console.WriteLine("Logged out successfully.");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        private static void ShowUserMenu()
        {
            Console.WriteLine($"\n--- User Menu ({_currentUser.Username}) ---");
            Console.WriteLine("1. View Available Books");
            Console.WriteLine("2. View My Borrowed Books");
            Console.WriteLine("3. Return Book");
            Console.WriteLine("4. Request Book");
            Console.WriteLine("5. Search Books");
            Console.WriteLine("6. Logout");
            Console.Write("Choose an option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ViewAvailableBooks();
                    break;
                case "2":
                    ViewMyBorrowedBooks();
                    break;
                case "3":
                    ReturnBook();
                    break;
                case "4":
                    RequestBook();
                    break;
                case "5":
                    SearchBooks();
                    break;
                case "6":
                    _currentUser = null;
                    Console.WriteLine("Logged out successfully.");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }

        private static void AddNewBook()
        {
            Console.WriteLine("\n--- Add New Book ---");
            Console.Write("Title: ");
            var title = Console.ReadLine();
            Console.Write("Author: ");
            var author = Console.ReadLine();
            Console.Write("Publication Year: ");
            if (int.TryParse(Console.ReadLine(), out int year))
            {
                Console.Write("ISBN (optional): ");
                var isbn = Console.ReadLine();

                if (_bookCRUD.AddBook(title, author, year, isbn))
                {
                    Console.WriteLine("Book added successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to add book.");
                }
            }
            else
            {
                Console.WriteLine("Invalid year format.");
            }
        }

        private static void ListAllBooks()
        {
            Console.WriteLine("\n--- All Books ---");
            var books = _bookCRUD.GetAllBooks();
            DisplayBooks(books);
        }

        private static void ViewAvailableBooks()
        {
            Console.WriteLine("\n--- Available Books ---");
            var books = _bookLending.GetAvailableBooks();
            DisplayBooks(books);
        }

        private static void ViewMyBorrowedBooks()
        {
            Console.WriteLine("\n--- My Borrowed Books ---");
            var books = _bookLending.GetBorrowedBooks(_currentUser.UserId);
            DisplayBooks(books);
        }

        private static void DisplayBooks(List<Book> books)
        {
            if (!books.Any())
            {
                Console.WriteLine("No books found.");
                return;
            }

            Console.WriteLine("ISBN\t\t\tTitle\t\t\tAuthor\t\t\tYear\tStatus");
            Console.WriteLine(new string('-', 100));

            foreach (var book in books)
            {
                Console.WriteLine($"{book.ISBN,-20}\t{book.Title,-20}\t{book.Author,-20}\t{book.PublicationYear}\t{book.Status}");
            }
        }

        private static void UpdateBook()
        {
            Console.WriteLine("\n--- Update Book ---");
            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();

            var book = _bookCRUD.GetBookByIsbn(isbn);
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            Console.WriteLine($"Current Title: {book.Title}");
            Console.Write("New Title (press Enter to keep current): ");
            var title = Console.ReadLine();
            if (!string.IsNullOrEmpty(title)) book.Title = title;

            Console.WriteLine($"Current Author: {book.Author}");
            Console.Write("New Author (press Enter to keep current): ");
            var author = Console.ReadLine();
            if (!string.IsNullOrEmpty(author)) book.Author = author;

            Console.WriteLine($"Current Year: {book.PublicationYear}");
            Console.Write("New Year (press Enter to keep current): ");
            var yearInput = Console.ReadLine();
            if (!string.IsNullOrEmpty(yearInput) && int.TryParse(yearInput, out int year))
            {
                book.PublicationYear = year;
            }

            Console.WriteLine($"Current ISBN: {book.ISBN}");
            Console.Write("New ISBN (press Enter to keep current): ");
            var newIsbn = Console.ReadLine();
            if (!string.IsNullOrEmpty(newIsbn)) book.ISBN = newIsbn;

            if (_bookCRUD.UpdateBook(isbn, book))
            {
                Console.WriteLine("Book updated successfully!");
            }
            else
            {
                Console.WriteLine("Failed to update book.");
            }
        }

        private static void DeleteBook()
        {
            Console.WriteLine("\n--- Delete Book ---");
            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();

            var book = _bookCRUD.GetBookByIsbn(isbn);
            if (book == null)
            {
                Console.WriteLine("Book not found.");
                return;
            }

            Console.WriteLine($"Are you sure you want to delete '{book.Title}'? (y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() == "y")
            {
                if (_bookCRUD.DeleteBook(isbn))
                {
                    Console.WriteLine("Book deleted successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to delete book. It might be currently borrowed.");
                }
            }
        }

        private static void LendBookToUser()
        {
            Console.WriteLine("\n--- Lend Book ---");
            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();
            Console.Write("Enter Username: ");
            var username = Console.ReadLine();

            var users = _userManager.GetAllUsers();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            if (_bookLending.LendBook(user.UserId, isbn))
            {
                Console.WriteLine("Book lent successfully!");
            }
            else
            {
                Console.WriteLine("Failed to lend book. Book might not exist or already borrowed.");
            }
        }

        private static void ProcessBookReturn()
        {
            Console.WriteLine("\n--- Process Return ---");
            Console.Write("Enter ISBN: ");
            var isbn = Console.ReadLine();
            Console.Write("Enter Username: ");
            var username = Console.ReadLine();

            var users = _userManager.GetAllUsers();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                Console.WriteLine("User not found.");
                return;
            }

            if (_bookLending.ReturnBook(user.UserId, isbn))
            {
                Console.WriteLine("Book returned successfully!");
            }
            else
            {
                Console.WriteLine("Failed to process return. Book might not be borrowed by this user.");
            }
        }

        private static void ReturnBook()
        {
            Console.WriteLine("\n--- Return Book ---");
            var borrowedBooks = _bookLending.GetBorrowedBooks(_currentUser.UserId);

            if (!borrowedBooks.Any())
            {
                Console.WriteLine("You have no borrowed books.");
                return;
            }

            Console.WriteLine("Your borrowed books:");
            DisplayBooks(borrowedBooks);

            Console.Write("Enter ISBN to return: ");
            var isbn = Console.ReadLine();

            if (_bookLending.ReturnBook(_currentUser.UserId, isbn))
            {
                Console.WriteLine("Book returned successfully!");
            }
            else
            {
                Console.WriteLine("Failed to return book.");
            }
        }

        private static void RequestBook()
        {
            Console.WriteLine("\n--- Request Book ---");
            var availableBooks = _bookLending.GetAvailableBooks();

            if (!availableBooks.Any())
            {
                Console.WriteLine("No books available for borrowing.");
                return;
            }

            Console.WriteLine("Available books:");
            DisplayBooks(availableBooks);

            Console.Write("Enter ISBN to request: ");
            var isbn = Console.ReadLine();

            if (_bookLending.RequestBook(_currentUser.UserId, isbn))
            {
                Console.WriteLine("Book request submitted successfully! Wait for admin approval.");
            }
            else
            {
                Console.WriteLine("Failed to submit request. You might have already requested this book.");
            }
        }

        private static void SearchBooks()
        {
            Console.WriteLine("\n--- Search Books ---");
            Console.Write("Enter search term (title, author, or ISBN): ");
            var searchTerm = Console.ReadLine();

            var books = _bookCRUD.SearchBooks(searchTerm);
            DisplayBooks(books);
        }

        private static void ShowPendingRequests()
        {
            var pendingRequests = _bookLending.GetPendingRequests();
            if (pendingRequests.Any())
            {
                Console.WriteLine($"\nYou have {pendingRequests.Count} pending book request(s)!");
            }
        }

        private static void ViewPendingRequests()
        {
            Console.WriteLine("\n--- Pending Requests ---");
            var requests = _bookLending.GetPendingRequests();

            if (!requests.Any())
            {
                Console.WriteLine("No pending requests.");
                return;
            }

            var users = _userManager.GetAllUsers();
            var books = _bookCRUD.GetAllBooks();

            foreach (var request in requests)
            {
                var user = users.FirstOrDefault(u => u.UserId == request.UserId);
                var book = books.FirstOrDefault(b => b.BookId == request.BookId);

                Console.WriteLine($"Request ID: {request.RequestId[..8]}");
                Console.WriteLine($"User: {user?.Username ?? "Unknown"}");
                Console.WriteLine($"Book: {book?.Title ?? "Unknown"}");
                Console.WriteLine($"Request Date: {request.RequestDate:yyyy-MM-dd HH:mm}");
                Console.WriteLine("---");
            }

            Console.Write("Enter Request ID to approve/reject (or press Enter to go back): ");
            var requestId = Console.ReadLine();

            if (!string.IsNullOrEmpty(requestId))
            {
                ProcessRequest(requestId);
            }
        }

        private static void ProcessRequest(string requestId)
        {
            Console.WriteLine("1. Approve");
            Console.WriteLine("2. Reject");
            Console.Write("Choose action: ");
            var action = Console.ReadLine();

            bool success = false;
            if (action == "1")
            {
                success = _bookLending.ApproveRequest(requestId, _currentUser.UserId);
                Console.WriteLine(success ? "Request approved successfully!" : "Failed to approve request.");
            }
            else if (action == "2")
            {
                success = _bookLending.RejectRequest(requestId, _currentUser.UserId);
                Console.WriteLine(success ? "Request rejected successfully!" : "Failed to reject request.");
            }
        }

        private static string ReadPassword()
        {
            try
            {
                // Check if we're in an interactive console environment
                if (Console.IsInputRedirected)
                {
                    // Non-interactive mode - just read the line directly
                    return Console.ReadLine() ?? "";
                }

                // Interactive mode - mask the password
                string password = "";
                ConsoleKeyInfo key;

                do
                {
                    key = Console.ReadKey(true);

                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password[0..^1];
                        Console.Write("\b \b");
                    }
                }
                while (key.Key != ConsoleKey.Enter);

                Console.WriteLine();
                return password;
            }
            catch
            {
                // Fallback to simple input if ReadKey fails
                return Console.ReadLine() ?? "";
            }
        }
    }
}