namespace TnieYuPackage.DesignPatterns
{
    /// <summary>
    /// Component is component in Visitor pattern. Has role is reverse direction.
    /// </summary>
    public interface IComponent : IVisitable
    {
        bool IsDisposed { get; set; }
        string Name { get; }
    }
}