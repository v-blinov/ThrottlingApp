namespace Basic.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class LimitRequestsAttribute : Attribute
{
    private int _timeWindow;
    private int _maxRequests;
    
    public int TimeWindow
    {
        get => _timeWindow;
        set => _timeWindow = value >= 1 ? value : 1;
    }
    
    public int MaxRequests
    {
        get => _maxRequests;
        set => _maxRequests = value >= 1 ? value : 1;
    }
}
