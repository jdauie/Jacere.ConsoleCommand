using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jacere.ConsoleCommand
{
    internal class CommandOptionInfo
    {
        private static readonly Dictionary<Type, Func<string, object>> Map = new Dictionary<Type, Func<string, object>>
        {
            {typeof(string), x => x},
            {typeof(int), x => int.Parse(x)},
            {typeof(long), x => long.Parse(x)},
            {typeof(double), x => double.Parse(x)},
            {typeof(Guid), x => Guid.Parse(x)},
        };

        public ConsoleCommandOptionAttribute Attribute { get; set; }
        public PropertyInfo Property { get; set; }

        private Type UnderlyingType => Nullable.GetUnderlyingType(Property.PropertyType) ?? Property.PropertyType;

        public bool IsValueType => Map.ContainsKey(UnderlyingType);

        public void SetValue(object instance, string value)
        {
            var underlyingPropType = UnderlyingType;

            var isStatic = Property.GetGetMethod(true).IsStatic;

            if (isStatic)
            {
                instance = null;
            }

            if (underlyingPropType == typeof(bool))
            {
                Property.SetValue(instance, true);
            }
            else if (IsValueType)
            {
                if (value == null)
                {
                    throw new Exception($"Unspecified value for `{Property.Name}`");
                }

                Property.SetValue(instance, Map[underlyingPropType](value));
            }
            else
            {
                throw new Exception($"Unsupported type for `{Property.Name}`");
            }
        }
    }
}