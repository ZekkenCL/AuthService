using System.IdentityModel.Tokens.Jwt;

using AuthServiceNamespace.Models;

namespace AuthServiceNamespace.Services
{
    public class TokenService : ITokenService
    {
        private readonly AuthDbContext _context;

        public TokenService(AuthDbContext context)
        {
            _context = context;
        }

        public bool ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return false;

            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null || _context.RevokedTokens.Any(t => t.Token == token))
                return false;

            return true;
        }

        public void RevokeToken(string token)
        {
            if (_context.RevokedTokens.Any(t => t.Token == token)) return;

            _context.RevokedTokens.Add(new RevokedToken
            {
                Token = token,
                RevokedAt = DateTime.UtcNow
            });
            _context.SaveChanges();
        }
    }
}
