# 📚 Library Management System

A comprehensive library management system that supports both admin and regular user operations with secure authentication and book management capabilities.

## Features

### Admin (Library Manager) Features
- Add new books to the library
- Update existing book information
- Delete books from the system
- List all books in the library
- Lend books to users
- Process book returns
- Review and approve/reject book borrowing requests

### Regular User Features
- View available books in the library
- View personal borrowed books list
- Return borrowed books
- Request to borrow available books

## Book Information Management
Each book contains:
- Book Title
- Author Name
- Publication Year
- Book Status (Available/Borrowed)
- ISBN (Optional)

## Security
- User passwords are stored securely using hashing/encryption
- Role-based access control for admin and regular users

## Data Storage
The system uses file-based storage with three separate files:
- User information
- Book information
- Borrowing records

Supported file formats: `.txt`, `.json`, or `.xml`

## System Architecture
- **UserManager**: Implements `IUserManager` interface for user authentication and management
- **BookManager**: Implements dual interfaces for:
  - Book CRUD operations (Create, Read, Update, Delete)
  - Book lending and return operations

## Getting Started

### Prerequisites
- [Programming language and version requirements will be added based on implementation]

### Installation
1. Clone the repository
2. [Setup instructions will be added based on implementation]

### Usage
1. Run the application
2. Login as Admin or Regular User
3. Navigate through the available options based on your role

## Project Structure
```
Library Management System/
├── README.md
├── PROJECT_SPECIFICATION.md
├── [Source code files to be added]
└── [Data files to be added]
```

## Contributing
This project is designed for educational purposes and future development enhancements.