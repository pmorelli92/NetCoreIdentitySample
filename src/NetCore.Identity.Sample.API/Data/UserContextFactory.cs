using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace NetCore.Identity.Sample.API.Data
{
    public class UserContextFactory : IDesignTimeDbContextFactory<UserContext>
    {
        private readonly IConfiguration _configuration;

        public UserContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public UserContext CreateDbContext(params string[] args)
        {
            var builder = new DbContextOptionsBuilder<UserContext>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            builder.UseSqlServer(connectionString);

            return new UserContext(builder.Options);
        }
    }
}