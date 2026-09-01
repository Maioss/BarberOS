using System.Security.Cryptography;
using BarberOS.Application.Shared;

namespace BarberOS.Infrastructure.Security
{
    public class RefreshTokenFactory : IRefreshTokenFactory
    {
        private const int TokenBytes = 32;

        public (string Token, string Hash) Create()
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes));
            return (token, Hash(token));
        }

        public string Hash(string token) =>
            Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
    }
}
