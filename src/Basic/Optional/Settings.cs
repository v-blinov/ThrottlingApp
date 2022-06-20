namespace Basic.Optional;

public record Settings
{
    public DefaultRateLimit DefaultRateLimit { get; init; } = null!;
}

public record DefaultRateLimit
{
    public int TimeWindow { get; init; }
    public int MaxRequests { get; init; }
}
