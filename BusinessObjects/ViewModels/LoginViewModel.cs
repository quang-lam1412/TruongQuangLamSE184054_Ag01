using System.ComponentModel.DataAnnotations;

namespace ViewModels
{
    public enum UserRole
    {
        Admin,
        Customer
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc số điện thoại")]
        public string Identifier { get; set; } = string.Empty; // Username hoặc Phone

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public UserRole Role { get; set; } = UserRole.Admin; // Giá trị mặc định
    }
}

