using UnityEngine;

namespace TnieYuPackage.CustomAttributes
{
    public class SetPropertyAttribute : PropertyAttribute
    {
        public string Name { get; private set; }

        public SetPropertyAttribute(string name)
        {
            this.Name = name;
        }
    }
}