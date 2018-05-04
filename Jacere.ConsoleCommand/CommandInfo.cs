using System;
using System.Reflection;

namespace Jacere.ConsoleCommand
{
    internal class CommandInfo
    {
        public Type Type { get; set; }
        public ConstructorInfo Constructor { get; set; }
        public ConsoleCommandAttribute Attribute { get; set; }

        public string Name => Attribute.Name ?? Type.Name;

        public IConsoleCommand Create()
        {
            return (IConsoleCommand)Constructor.Invoke(new object[0]);
        }
    }
}