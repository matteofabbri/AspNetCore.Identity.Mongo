using System.ComponentModel.DataAnnotations;

namespace Example.DefaultUser.Models.AccountViewModels
{
    public class ExternalLoginViewModel
    {
        [Required] [EmailAddress] public string Email { get; set; }
    }
}