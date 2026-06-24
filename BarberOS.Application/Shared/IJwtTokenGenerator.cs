using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{

    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}
