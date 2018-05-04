using System;

namespace Jacere.ConsoleCommand
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {
        public ConsoleCommandAttribute(string name = null, string shortDescription = null, string longDescription = null)
        {
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
        }

        public string Name { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }
    }
}
