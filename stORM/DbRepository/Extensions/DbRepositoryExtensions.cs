namespace stORM.DbRepository.Extensions;

public static class DbRepositoryExtensions
{
    public static IEnumerable<TOuter> ThenJoin<TOuter, TKey>(this IEnumerable<TOuter> outer, Func<TOuter, TKey> keySelector) => outer;
    public static TOuter ThenJoin<TOuter, TKey>(this TOuter outer, Func<TOuter, TKey> keySelector) => outer;
    public static TOuter Where<TOuter, TKey>(this TOuter outer, Func<TOuter, TKey> keySelector) => outer;
}
