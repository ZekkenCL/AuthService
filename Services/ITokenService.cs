namespace AuthServiceNamespace.Services
{
    public interface ITokenService
    {
        bool ValidateToken(string token);
        void RevokeToken(string token);
    }
}
