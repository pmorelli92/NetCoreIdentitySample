using System;

namespace NetCore.Identity.Sample.API.Tokens
{
    public class TokenResponse
    {
        public Guid UserId { get; set; }

        public string AuthToken { get; set; }

        public long ExpirationSeconds { get; set; }

        public TokenResponse(
            Guid userId,
            string authToken,
            long expirationSeconds)
        {
            UserId = userId;
            AuthToken = authToken;
            ExpirationSeconds = expirationSeconds;
        }
    }
}