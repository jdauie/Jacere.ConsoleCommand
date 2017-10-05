using System;

namespace Jacere.ConsoleCommand
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {
        public ConsoleCommandAttribute(string name = null, string description = null, bool admin = false)
        {
            Name = name;
            Description = description;
            Admin = admin;
        }

        public string Name { get; }

        public string Description { get; }

        public bool Admin { get; }
    }
}
