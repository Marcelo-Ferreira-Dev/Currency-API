using API.Contracts;
using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(AppDbContext context)
{
    public async Task<UserResponse?> HandleAsync(GetUserByIdQuery query, CancellationToken ct = default)
    {
        return await context.Users.AsNoTracking().Where(u => u.Id == query.Id)
            .Select(u => new UserResponse(u.Id, u.Name, u.Email, u.IsActive)).SingleOrDefaultAsync(ct);
    }
}
