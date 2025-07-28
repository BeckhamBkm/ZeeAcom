using ZeeAcom.Common.Models;
namespace ZeeAcom.Common.Extensions;

public static class PagingExtension
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagingArgs? pagingArgs)
    {
        var args = pagingArgs ?? PagingArgs.Default;

        return args.UsePaging
            ? query.Skip(args.Offset).Take(args.Limit)
            : query;
    }
}
