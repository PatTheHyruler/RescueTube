using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account;

public class LoginModel
{
    public string? ReturnUrl { get; set; }
    public required InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        public required string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; } = true;
    }
}