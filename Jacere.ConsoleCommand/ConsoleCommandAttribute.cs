using System;

namespace Jacere.ConsoleCommand
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {
        public ConsoleCommandAttribute(string name = null, string shortDescription = null, string longDescription = null, bool admin = false)
        {
            Name = name;
            ShortDescription = shortDescription;
            LongDescription = longDescription;
            Admin = admin;
        }

        public string Name { get; }

        public string ShortDescription { get; }

        public string LongDescription { get; }

        public bool Admin { get; }
    }
}
