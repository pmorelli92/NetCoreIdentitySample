using System.Security.Claims;

namespace NetCore.Identity.Sample.API.JWT
{
    public interface IJwtFactory
    {
        string GenerateEncodedToken(string userName, ClaimsIdentity identity);

        ClaimsIdentity GenerateClaimsIdentity(string userName, string id);
    }
}