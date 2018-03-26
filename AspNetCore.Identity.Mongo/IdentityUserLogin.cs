using Microsoft.AspNetCore.Identity;
using Mongolino.Attributes;

namespace AspNetCore.Identity.Mongo
{
    public class IdentityUserLogin
    {
        public IdentityUserLogin() { }

        public IdentityUserLogin(string loginProvider, string providerKey, string providerDisplayName)
        {
            LoginProvider = loginProvider;
            ProviderDisplayName = providerDisplayName;
            ProviderKey = providerKey;
        }

        [AscendingIndex]
        public string UserId { get; set; }

        [AscendingIndex]
        public string LoginProvider { get; set; }

        public string ProviderDisplayName { get; set; }

        [AscendingIndex]
        public string ProviderKey { get; set; }

        public UserLoginInfo ToUserLoginInfo()
        {
            return new UserLoginInfo(LoginProvider, ProviderKey, ProviderDisplayName);
        }
    }
}
