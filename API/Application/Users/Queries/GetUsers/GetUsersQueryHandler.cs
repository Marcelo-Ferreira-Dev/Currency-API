using API.Contracts;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler(AppDbContext context)
{
    public async Task<List<UserResponse>> HandleAsync(GetUsersQuery query, CancellationToken ct = default)
    {
        var users = context.Users.AsNoTracking();
        if (query.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == query.IsActive.Value);
        }

        return await users.OrderBy(u => u.Id).Select(u => new UserResponse(u.Id, u.Name, u.Email, u.IsActive)).ToListAsync(ct);
    }
}
