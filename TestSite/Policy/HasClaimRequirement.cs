using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Policy
{
    public class HasClaimRequirement : IAuthorizationRequirement
    {
        public string UserClaims { get; set; }
        public HasClaimRequirement(string userClaims)
        {
            UserClaims = userClaims?? throw new ArgumentNullException(nameof(userClaims));
        }
    }
}
