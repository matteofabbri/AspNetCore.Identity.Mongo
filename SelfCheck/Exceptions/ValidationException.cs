using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace SampleSite.Exceptions
{
    public class ValidationException : Exception
    {
        private IEnumerable<IdentityError> errors;

        public ValidationException(IEnumerable<IdentityError> errors):base(string.Join(" ",errors.Select(x=>x.Description)))
        {
            
        }
    }
}
