using System.ComponentModel.DataAnnotations;

namespace TestSite.Services.Identity.AccountViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}