using System.ComponentModel.DataAnnotations;

namespace Example.CustomUser.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
