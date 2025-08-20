using LibraryManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LibraryManagementSystem.DataAccess
{
    public class FileDataService
    {
        private readonly string _dataDirectory;
        private readonly string _usersFilePath;
        private readonly string _booksFilePath;
        private readonly string _borrowingRecordsFilePath;
        private readonly string _borrowingRequestsFilePath;

        public FileDataService(string dataDirectory = "Data")
        {
            _dataDirectory = dataDirectory;
            _usersFilePath = Path.Combine(_dataDirectory, "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/users.json");
            _booksFilePath = Path.Combine(_dataDirectory, "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/books.json");
            _borrowingRecordsFilePath = Path.Combine(_dataDirectory, "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/borrowing_records.json");
            _borrowingRequestsFilePath = Path.Combine(_dataDirectory, "/Users/efeugur/Desktop/Kütüphane Sistemi/Data/borrowing_requests.json");

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

            InitializeDataFiles();
        }

        private void InitializeDataFiles()
        {
            if (!File.Exists(_usersFilePath))
            {
                SaveUsers(new List<User>());
            }

            if (!File.Exists(_booksFilePath))
            {
                SaveBooks(new List<Book>());
            }

            if (!File.Exists(_borrowingRecordsFilePath))
            {
                SaveBorrowingRecords(new List<BorrowingRecord>());
            }

            if (!File.Exists(_borrowingRequestsFilePath))
            {
                SaveBorrowingRequests(new List<BorrowingRequest>());
            }
        }

        public List<User> LoadUsers()
        {
            try
            {
                if (!File.Exists(_usersFilePath))
                    return new List<User>();

                var json = File.ReadAllText(_usersFilePath);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }

        public void SaveUsers(List<User> users)
        {
            try
            {
                var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_usersFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users: {ex.Message}");
            }
        }

        public List<Book> LoadBooks()
        {
            try
            {
                if (!File.Exists(_booksFilePath))
                    return new List<Book>();

                var json = File.ReadAllText(_booksFilePath);
                return JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading books: {ex.Message}");
                return new List<Book>();
            }
        }

        public void SaveBooks(List<Book> books)
        {
            try
            {
                var json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_booksFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving books: {ex.Message}");
            }
        }

        public List<BorrowingRecord> LoadBorrowingRecords()
        {
            try
            {
                if (!File.Exists(_borrowingRecordsFilePath))
                    return new List<BorrowingRecord>();

                var json = File.ReadAllText(_borrowingRecordsFilePath);
                return JsonSerializer.Deserialize<List<BorrowingRecord>>(json) ?? new List<BorrowingRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrowing records: {ex.Message}");
                return new List<BorrowingRecord>();
            }
        }

        public void SaveBorrowingRecords(List<BorrowingRecord> records)
        {
            try
            {
                var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_borrowingRecordsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving borrowing records: {ex.Message}");
            }
        }

        public List<BorrowingRequest> LoadBorrowingRequests()
        {
            try
            {
                if (!File.Exists(_borrowingRequestsFilePath))
                    return new List<BorrowingRequest>();

                var json = File.ReadAllText(_borrowingRequestsFilePath);
                return JsonSerializer.Deserialize<List<BorrowingRequest>>(json) ?? new List<BorrowingRequest>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading borrowing requests: {ex.Message}");
                return new List<BorrowingRequest>();
            }
        }

        public void SaveBorrowingRequests(List<BorrowingRequest> requests)
        {
            try
            {
                var json = JsonSerializer.Serialize(requests, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_borrowingRequestsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving borrowing requests: {ex.Message}");
            }
        }
    }
}