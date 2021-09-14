namespace Orthogonal.Persistence.LiteDB
{
    public interface LiteDBClientConfiguration
    {
        string DatabaseLoclation { get; }
        bool ReadOnly { get; }
    }
}