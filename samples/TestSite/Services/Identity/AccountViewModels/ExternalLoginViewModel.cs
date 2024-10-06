using System.ComponentModel.DataAnnotations;

namespace TestSite.Services.Identity.AccountViewModels;

public class ExternalLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}