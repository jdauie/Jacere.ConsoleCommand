using System;
using JetBrains.Annotations;

namespace Jacere.ConsoleCommand
{
    [AttributeUsage(AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class ConsoleCommandOptionAttribute : Attribute
    {
        public ConsoleCommandOptionAttribute(string option, string shortOption = null, bool required = false, string description = null)
        {
            if (shortOption != null)
            {
                if (shortOption.Length != 1 || shortOption[0] < 'a' || shortOption[0] > 'z')
                {
                    throw new ArgumentException("Short option must be a single letter [a-z]", nameof(shortOption));
                }
            }

            Option = option;
            ShortOption = shortOption;
            Required = required;
            Description = description;
        }

        public string Option { get; }

        public string ShortOption { get; }

        public bool Required { get; }

        public string Description { get; }
    }
}
