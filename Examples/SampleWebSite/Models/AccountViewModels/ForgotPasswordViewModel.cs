using System.ComponentModel.DataAnnotations;

namespace Example.DefaultUser.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required] [EmailAddress] public string Email { get; set; }
    }
}