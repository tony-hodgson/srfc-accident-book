namespace AccidentBook.API.Services;

/// <summary>Tracks IDs of accident rows inserted by <see cref="AccidentSeedHostedService"/> for removal on shutdown.</summary>
internal sealed class AccidentSeedState
{
    private readonly object _lock = new();
    private List<int> _ids = new();

    public void SetIds(IEnumerable<int> ids)
    {
        lock (_lock)
            _ids = ids.ToList();
    }

    public IReadOnlyList<int> GetIds()
    {
        lock (_lock)
            return _ids.ToList();
    }
}
