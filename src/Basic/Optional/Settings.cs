namespace Basic.Optional;

public record Settings
{
    public DefaultRateLimit DefaultRateLimit { get; init; } = null!;
    
    public IEnumerable<string>? IpWhitelist { get; init; }
    public IEnumerable<string>? ClientWhitelist { get; init; }
    
    public IEnumerable<IndividualLimit>? IndividualLimits { get; init; }
}

public record DefaultRateLimit
{
    public int TimeWindow { get; init; }
    public int MaxRequests { get; init; }
}

public record IndividualLimit
{
    public string Ip { get; init; } = null!;
    public DefaultRateLimit RateLimit { get; init; } = null!;
}
