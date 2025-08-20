# 📚 Library Management System - Technical Specification

## 1. System Overview
A file-based library management system with role-based access control supporting administrative and user operations for book management and borrowing.

## 2. User Roles and Permissions

### 2.1 Admin (Library Manager)
**Primary Responsibilities:**
- Complete book inventory management
- User lending operations
- System administration

**Specific Capabilities:**
- Add new books to the library database
- List and search existing books
- Update book information (title, author, year, ISBN)
- Delete books from the system
- Lend books to registered users
- Process book returns from users
- Review borrowing requests from users
- Approve or reject borrowing requests

### 2.2 Regular Users
**Primary Responsibilities:**
- Personal book management
- Library browsing

**Specific Capabilities:**
- View list of personally borrowed books
- Browse available books in the library
- Return borrowed books
- Submit requests to borrow available books

## 3. Data Models

### 3.1 Book Entity
**Required Fields:**
- `title`: String - Book title
- `author`: String - Author name
- `publication_year`: Integer - Year of publication
- `status`: Enum - AVAILABLE | BORROWED
- `isbn`: String (Optional) - International Standard Book Number

**Additional System Fields:**
- `book_id`: Unique identifier
- `created_date`: Timestamp
- `last_updated`: Timestamp

### 3.2 User Entity
**Required Fields:**
- `user_id`: Unique identifier
- `username`: String
- `password_hash`: String (encrypted/hashed)
- `role`: Enum - ADMIN | REGULAR_USER
- `created_date`: Timestamp

### 3.3 Borrowing Record Entity
**Required Fields:**
- `record_id`: Unique identifier
- `user_id`: Foreign key to User
- `book_id`: Foreign key to Book
- `borrow_date`: Timestamp
- `due_date`: Timestamp
- `return_date`: Timestamp (nullable)
- `status`: Enum - ACTIVE | RETURNED | OVERDUE

### 3.4 Borrowing Request Entity (Advanced Feature)
**Required Fields:**
- `request_id`: Unique identifier
- `user_id`: Foreign key to User
- `book_id`: Foreign key to Book
- `request_date`: Timestamp
- `status`: Enum - PENDING | APPROVED | REJECTED
- `admin_response_date`: Timestamp (nullable)
- `admin_id`: Foreign key to User (nullable)

## 4. System Architecture

### 4.1 Interface Definitions

#### IUserManager Interface
```
Methods:
- authenticateUser(username, password): User
- createUser(userData): boolean
- updateUser(userId, userData): boolean
- deleteUser(userId): boolean
- getUserById(userId): User
- getAllUsers(): List<User>
```

#### IBookCRUD Interface
```
Methods:
- addBook(bookData): boolean
- updateBook(bookId, bookData): boolean
- deleteBook(bookId): boolean
- getBookById(bookId): Book
- getAllBooks(): List<Book>
- searchBooks(criteria): List<Book>
```

#### IBookLending Interface
```
Methods:
- lendBook(userId, bookId): boolean
- returnBook(userId, bookId): boolean
- getBorrowedBooks(userId): List<Book>
- getAvailableBooks(): List<Book>
- getBorrowingHistory(userId): List<BorrowingRecord>
```

### 4.2 Class Structure

#### UserManager Class
**Implements:** IUserManager
**Responsibilities:**
- User authentication and session management
- User CRUD operations
- Password hashing and validation
- Role-based access control

#### BookManager Class
**Implements:** IBookCRUD, IBookLending
**Responsibilities:**
- Book inventory management
- Lending and return operations
- Book availability tracking
- Borrowing record maintenance

## 5. Data Storage Specification

### 5.1 File Structure
**Three separate data files:**
1. `users.[ext]` - User information storage
2. `books.[ext]` - Book information storage
3. `borrowing_records.[ext]` - Lending/borrowing transaction records

### 5.2 Supported File Formats
- `.txt` - Plain text with delimiter separation
- `.json` - JSON format for structured data
- `.xml` - XML format with defined schema

### 5.3 Data Persistence Requirements
- Atomic file operations to prevent data corruption
- Backup mechanism for data recovery
- Data validation before file write operations

## 6. Advanced Features

### 6.1 Book Request System
**Workflow:**
1. Regular user submits borrowing request for available book
2. Request is stored with PENDING status
3. Admin views pending requests upon login
4. Admin can approve or reject requests
5. Approved requests automatically update book status
6. Users are notified of request status changes

### 6.2 Security Implementation
**Password Security:**
- Passwords stored using secure hashing algorithm (SHA-256 minimum)
- Salt-based hashing to prevent rainbow table attacks
- Option for encryption-based storage

**Session Management:**
- User authentication tokens
- Role-based method access control
- Secure logout functionality

## 7. System Constraints

### 7.1 Functional Constraints
- Single book copy per title (no quantity management)
- File-based storage only (no database integration)
- Command-line or simple GUI interface

### 7.2 Non-Functional Requirements
- Response time: < 2 seconds for file operations
- Data integrity: Atomic operations for data consistency
- Scalability: Support up to 10,000 books and 1,000 users

## 8. Future Enhancement Opportunities

### 8.1 Immediate Extensions
- Multiple book copies management
- Due date notifications
- Fine calculation system
- Book reservation system

### 8.2 Advanced Extensions
- Database migration capability
- Web-based interface
- Email notifications
- Reporting and analytics
- Book recommendation system

## 9. Development Tasks

### Phase 1: Core Infrastructure
- [ ] Implement data models and interfaces
- [ ] Create file I/O operations
- [ ] Implement basic user authentication
- [ ] Develop book CRUD operations

### Phase 2: Business Logic
- [ ] Implement lending/return functionality
- [ ] Add user management features
- [ ] Create admin operations interface
- [ ] Implement security features

### Phase 3: Advanced Features
- [ ] Book request system
- [ ] Advanced search functionality
- [ ] Data validation and error handling
- [ ] System testing and optimization

### Phase 4: Enhancement
- [ ] User interface improvements
- [ ] Performance optimization
- [ ] Documentation completion
- [ ] Deployment preparation