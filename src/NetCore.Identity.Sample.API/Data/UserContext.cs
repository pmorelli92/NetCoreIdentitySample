using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace NetCore.Identity.Sample.API.Data
{
    public class UserContext : IdentityDbContext<Entities.User, Entities.Role, Guid>
    {
        public UserContext(DbContextOptions options) : base(options)
        {
        }
    }
}