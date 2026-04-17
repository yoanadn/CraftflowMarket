using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.ViewModels;

public class ProfileEditViewModel
{
    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Име")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [EmailAddress(ErrorMessage = "Въведи валиден имейл адрес.")]
    [RegularExpression(@"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$", ErrorMessage = "Имейлът трябва да бъде във формат name@example.com.")]
    [Display(Name = "Имейл")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Въведи валиден телефонен номер.")]
    [Display(Name = "Телефон")]
    public string? PhoneNumber { get; set; }

    [MaxLength(400, ErrorMessage = "Биографията може да е до 400 символа.")]
    [Display(Name = "Биография")]
    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }
}
