using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace NetCore.Identity.Sample.API.Tokens
{
    public class JwtFactory
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SigningCredentials _signingCredentials;

        public JwtFactory(
            string issuer,
            string audience,
            SigningCredentials signingCredentials)
        {
            _issuer = issuer;
            _audience = audience;
            _signingCredentials = signingCredentials;
        }

        public string GenerateToken(
            string userId,
            string userName,
            IEnumerable<string> roles,
            TimeSpan tokenDuration)
        {
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, userName),
                new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("uid", userId)
             }.Concat(roles.Select(e => new Claim("roles", e)));

            var jwtToken =
                new JwtSecurityToken(
                    _issuer, _audience, claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.Add(tokenDuration),
                    signingCredentials: _signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
    }
}