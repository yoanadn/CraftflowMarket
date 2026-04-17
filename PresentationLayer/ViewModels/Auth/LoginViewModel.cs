using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.ViewModels.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Полето е задължително.")]
    [Display(Name = "Username или имейл")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [DataType(DataType.Password)]
    [Display(Name = "Парола")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
