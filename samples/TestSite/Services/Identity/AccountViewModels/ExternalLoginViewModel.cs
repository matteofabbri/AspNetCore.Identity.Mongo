using System.ComponentModel.DataAnnotations;

namespace SampleSite.Identity.AccountViewModels;

public class ExternalLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}