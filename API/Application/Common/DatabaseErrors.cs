using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
namespace API.Application.Common;
public static class DatabaseErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 };
}
