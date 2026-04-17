using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.ViewModels.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Полето е задължително.")]
    [MinLength(3, ErrorMessage = "Username трябва да е поне 3 символа.")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [EmailAddress(ErrorMessage = "Въведи валиден имейл адрес.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Имейлът трябва да бъде във формат name@example.com.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Паролата трябва да е поне 6 символа.")]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
    [Display(Name = "Потвърди парола")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Име")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;
}
