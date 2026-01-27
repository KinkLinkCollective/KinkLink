using KinkLinkCommon.Database;

namespace KinkLinkServer.Extensions;

/// <summary>
/// Extension methods for converting between database models and domain models
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Converts a GetAllPairsForProfileRow to a Pair domain model
    /// </summary>
    public static Pair ToPair(this QueriesSql.GetAllPairsForProfileRow row)
    {
        return new Pair(
            row.Id,
            row.PairId,
            row.Expires,
            row.ToggleTimerLocks,
            row.TogglePermanentLocks,
            row.ToggleGarbler,
            row.LockGarbler,
            row.ToggleChannels,
            row.LockChannels,
            row.ApplyGag,
            row.LockGag,
            row.UnlockGag,
            row.RemoveGag,
            row.ApplyWardrobe,
            row.LockWardrobe,
            row.UnlockWardrobe,
            row.RemoveWardrobe,
            row.ApplyMoodles,
            row.LockMoodles,
            row.UnlockMoodles,
            row.RemoveMoodles
        );
    }

    /// <summary>
    /// Converts a collection of GetAllPairsForProfileRow to Pair domain models
    /// </summary>
    public static IEnumerable<Pair> ToPairs(this IEnumerable<QueriesSql.GetAllPairsForProfileRow> rows)
    {
        return rows.Select(ToPair);
    }
}
