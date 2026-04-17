using System.ComponentModel.DataAnnotations;

namespace PresentationLayer.ViewModels;

public class CheckoutPageViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; set; } = [];

    public decimal Subtotal { get; set; }

    public decimal Shipping { get; set; }

    public decimal Total => Subtotal + Shipping;

    public CheckoutInputViewModel Input { get; set; } = new();
}

public class CheckoutInputViewModel
{
    [Required(ErrorMessage = "Полето е задължително.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Името и фамилията трябва да са между 3 и 80 символа.")]
    [RegularExpression(@"^[\p{L}][\p{L}\s\-']+$", ErrorMessage = "Името и фамилията трябва да съдържат само букви, интервали, апостроф или тире.")]
    [Display(Name = "Име и фамилия")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [RegularExpression(@"^\+?[0-9]{8,15}$", ErrorMessage = "Телефонният номер трябва да съдържа между 8 и 15 цифри (по избор с + отпред).")]
    [Display(Name = "Телефонен номер")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Градът трябва да е между 2 и 50 символа.")]
    [RegularExpression(@"^[\p{L}][\p{L}\s\-']+$", ErrorMessage = "Градът трябва да съдържа само букви.")]
    [Display(Name = "Град")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [StringLength(120, MinimumLength = 5, ErrorMessage = "Улицата и номерът трябва да са между 5 и 120 символа.")]
    [RegularExpression(@"^(?=.*\p{L})(?=.*\d)[\p{L}\d\s\.,\-\/№]+$", ErrorMessage = "Улицата трябва да съдържа букви и номер и може да включва само букви, цифри, интервали и символите . , - / №")]
    [Display(Name = "Улица и номер")]
    public string StreetAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полето е задължително.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Пощенският код трябва да е точно 4 цифри.")]
    [Display(Name = "Пощенски код")]
    public string PostalCode { get; set; } = string.Empty;

    [Display(Name = "Начин на плащане")]
    public string PaymentMethod { get; set; } = "cash_on_delivery";

    public string? Notes { get; set; }
}
