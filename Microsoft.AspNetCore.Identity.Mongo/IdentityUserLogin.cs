using Mongolino;

namespace Microsoft.AspNetCore.Identity.Mongo
{
    public class IdentityUserLogin :DBObject<IdentityUserLogin>
    {
        public IdentityUserLogin() { }

        public IdentityUserLogin(string loginProvider, string providerKey, string providerDisplayName)
        {
            LoginProvider = loginProvider;
            ProviderDisplayName = providerDisplayName;
            ProviderKey = providerKey;
        }

        public IdentityUserLogin(ApplicationUser user, UserLoginInfo login)
        {
            UserId = user.Id;
            LoginProvider = login.LoginProvider;
            ProviderDisplayName = login.ProviderDisplayName;
            ProviderKey = login.ProviderKey;
        }

        public string UserId { get; set; }
        public string LoginProvider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderKey { get; set; }

        public UserLoginInfo ToUserLoginInfo()
        {
            return new UserLoginInfo(LoginProvider, ProviderKey, ProviderDisplayName);
        }
    }
}
