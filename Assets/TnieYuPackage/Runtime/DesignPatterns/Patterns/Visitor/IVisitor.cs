namespace TnieYuPackage.DesignPatterns
{
    public interface IVisitor
    {
        void Visit(IVisitable visitable);
    }

    public interface IVisitor<in T> : IVisitor where T : IVisitable
    {
        void IVisitor.Visit(IVisitable visitable)
        {
            if (visitable is T tAcceptable)
            {
                Visit(tAcceptable);
            }
        }

        void Visit(T acceptable);
    }
}