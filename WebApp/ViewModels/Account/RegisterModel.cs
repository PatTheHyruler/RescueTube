using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account;

public class RegisterModel
{
    public InputModel Input { get; set; } = default!;
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required] public string UserName { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = default!;

        public bool RememberMe { get; set; } = true;
    }
}