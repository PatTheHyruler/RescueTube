using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account;

public class RegisterModel
{
    public required InputModel Input { get; set; }
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required] public required string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public required string ConfirmPassword { get; set; }

        public bool RememberMe { get; set; } = true;
    }
}