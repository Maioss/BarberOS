namespace BarberOS.Application.Shared
{
    public interface IRefreshTokenFactory
    {
        /// <summary>Devuelve el token en claro, que solo viaja al cliente, y su hash.</summary>
        (string Token, string Hash) Create();

        string Hash(string token);
    }
}
