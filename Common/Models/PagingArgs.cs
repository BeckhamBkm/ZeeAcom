namespace ZeeAcom.Common.Models;
public class PagingArgs
{
    private const int DefaultLimit = 20;
    private const int DefaultOffset = 0;

    private int _limit = DefaultLimit;

    public static readonly PagingArgs NoPaging = new() { UsePaging = false };
    public static readonly PagingArgs Default = new() { UsePaging = true, _limit = DefaultLimit, Offset = DefaultOffset };
    public static readonly PagingArgs FirstItem = new() { UsePaging = true, _limit = 1, Offset = DefaultOffset };

    public int Offset { get; set; } = DefaultOffset;
    public int Limit
    {
        get => _limit;
        set => _limit = value > 0 ? value : DefaultLimit;
    }
    public bool UsePaging { get; set; } = true;
}
