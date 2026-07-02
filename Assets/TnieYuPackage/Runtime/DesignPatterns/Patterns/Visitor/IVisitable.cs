namespace TnieYuPackage.DesignPatterns
{
    public interface IVisitable
    {
        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}