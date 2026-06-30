using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;

namespace BarberOS.Application.Users.UseCases
{
    public class ListUsersUseCase
    {
        private readonly IUserRepository _users;

        public ListUsersUseCase(IUserRepository users) => _users = users;

        public async Task<PagedResult<UserDto>> ExecuteAsync(UserFilter filter, CancellationToken ct = default)
        {
            var page = await _users.ListAsync(filter, ct);

            var items = page.Items.Select(u => new UserDto(
                u.Id, u.Email, u.FullName, u.Phone, u.PhotoUrl, u.Role, u.BarbershopId, u.IsActive, u.CreatedAt
            )).ToList();

            return new PagedResult<UserDto>(items, page.Page, page.PageSize, page.TotalCount);
        }
    }
}
