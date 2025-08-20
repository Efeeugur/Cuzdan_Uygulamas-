using LibraryManagementSystem.BusinessLogic.Interfaces;
using LibraryManagementSystem.DataAccess;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Utils;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.BusinessLogic.Services
{
    public class UserManager : IUserManager
    {
        private readonly FileDataService _dataService;

        public UserManager(FileDataService dataService)
        {
            _dataService = dataService;
        }

        public User AuthenticateUser(string username, string password)
        {
            var users = _dataService.LoadUsers();
            var user = users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());

            if (user != null && SecurityHelper.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public bool CreateUser(string username, string password, UserRole role)
        {
            var users = _dataService.LoadUsers();

            if (users.Any(u => u.Username.ToLower() == username.ToLower()))
            {
                return false;
            }

            var hashedPassword = SecurityHelper.HashPassword(password);
            var newUser = new User(username, hashedPassword, role);
            users.Add(newUser);
            _dataService.SaveUsers(users);

            return true;
        }

        public bool UpdateUser(string userId, User userData)
        {
            var users = _dataService.LoadUsers();
            var user = users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return false;

            user.Username = userData.Username;
            user.Role = userData.Role;

            _dataService.SaveUsers(users);
            return true;
        }

        public bool DeleteUser(string userId)
        {
            var users = _dataService.LoadUsers();
            var user = users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
                return false;

            users.Remove(user);
            _dataService.SaveUsers(users);
            return true;
        }

        public User GetUserById(string userId)
        {
            var users = _dataService.LoadUsers();
            return users.FirstOrDefault(u => u.UserId == userId);
        }

        public List<User> GetAllUsers()
        {
            return _dataService.LoadUsers();
        }

        public bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            var users = _dataService.LoadUsers();
            var user = users.FirstOrDefault(u => u.UserId == userId);

            if (user == null || !SecurityHelper.VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = SecurityHelper.HashPassword(newPassword);
            _dataService.SaveUsers(users);
            return true;
        }
    }
}