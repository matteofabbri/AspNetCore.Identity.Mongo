using System.ComponentModel.DataAnnotations;

namespace Example.CustomUser.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
