using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account;

public class LoginModel
{
    public string? ReturnUrl { get; set; }
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        public string UserName { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        public bool RememberMe { get; set; } = true;
    }
}