using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account;

public class LoginModel
{
    public string? ReturnUrl { get; set; }
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
    }
}