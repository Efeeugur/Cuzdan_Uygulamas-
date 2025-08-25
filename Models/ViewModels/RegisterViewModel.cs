using System.ComponentModel.DataAnnotations;

namespace Cüzdan_Uygulaması.Models.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}