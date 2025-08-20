using System;

namespace LibraryManagementSystem.Models
{
    public enum UserRole
    {
        Admin,
        RegularUser
    }

    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedDate { get; set; }

        public User()
        {
            UserId = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
        }

        public User(string username, string passwordHash, UserRole role) : this()
        {
            Username = username;
            PasswordHash = passwordHash;
            Role = role;
        }
    }
}