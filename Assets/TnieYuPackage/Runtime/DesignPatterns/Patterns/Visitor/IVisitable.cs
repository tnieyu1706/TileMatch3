namespace TnieYuPackage.DesignPatterns
{
    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }

    public interface IVisitable<in T> : IVisitable where T : IVisitor
    {
        void IVisitable.Accept(IVisitor visitor)
        {
            if (visitor is T tVisitor)
            {
                Accept(tVisitor);
            }
        }

        void Accept(T visitor);
    }
}