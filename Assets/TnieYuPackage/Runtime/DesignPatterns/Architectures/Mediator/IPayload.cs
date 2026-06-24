using UnityEngine;

namespace TnieYuPackage.DesignPatterns
{
    /// <summary>
    /// When at [Level 3]. Payload is data & handler
    /// Data: the information to be sent
    /// Handler: the logic to be executed on the receiver's side
    /// </summary>
    public interface IPayload : IVisitor<IComponent>
    {
        object Data { get; set; }
    }

    public class StringPayload : IPayload
    {
        public IComponent Source { get; set; }
        public string Data { get; set; }

        public void Visit(IComponent acceptable)
        {
            Debug.Log($"Payload from {Source.Name} to {acceptable.Name}: {Data}");
            // Execute logic on component
        }

        object IPayload.Data
        {
            get => Data;
            set => Data = (string)value;
        }
    }
}