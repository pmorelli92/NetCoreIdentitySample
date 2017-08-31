using Microsoft.AspNetCore.Identity;
using System;

namespace NetCore.Identity.Sample.API.Entities
{
    public class User : IdentityUser<Guid>
    {
        public bool IsAnonymous { get; set; }
    }
}