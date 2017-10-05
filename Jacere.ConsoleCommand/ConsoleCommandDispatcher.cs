using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jacere.ConsoleCommand
{
    public class ConsoleCommandDispatcher
    {
        public static void DispatchCommand(IEnumerable<Assembly> assemblies, string commandName = null)
        {
            DispatchCommand(assemblies.SelectMany(x => x.GetTypes()), commandName);
        }
        
        private static void DispatchCommand(IEnumerable<Type> types, string commandName)
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToList();
            if (commandName == null)
            {
                commandName = args.First();
                args = args.Skip(1).ToList();
            }

            var commandInfo = types
                .Where(x => typeof(IConsoleCommand).IsAssignableFrom(x))
                .Where(x => x.IsClass && !x.IsAbstract)
                .Select(x => new
                {
                    Type = x,
                    Constructor = x.GetConstructor(new Type[0]),
                    Attribute = (ConsoleCommandAttribute)x.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).SingleOrDefault()
                })
                .Where(x => x.Attribute != null && x.Constructor != null)
                .SingleOrDefault(x => string.Equals(x.Attribute.Name ?? x.Type.Name, commandName, StringComparison.OrdinalIgnoreCase));

            if (commandInfo == null)
            {
                throw new Exception("no command");
            }

            var command = (IConsoleCommand)commandInfo.Constructor.Invoke(new object[0]);

            BindCommandArgs(command.GetType(), command, args);

            command.Execute().GetAwaiter().GetResult();
        }

        private static void BindCommandArgs(Type target, object instance, IEnumerable<string> commandArgs)
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Public;

            if (instance != null)
            {
                flags |= BindingFlags.Instance;
            }
            else
            {
                flags |= BindingFlags.Static;
            }

            var props = target.GetProperties(flags);

            var optionMap = new Dictionary<string, CommandOptionInfo>();
            var shortOptionMap = new Dictionary<char, string>();

            foreach (var prop in props)
            {
                var attr = (ConsoleCommandOptionAttribute)prop.GetCustomAttributes(typeof(ConsoleCommandOptionAttribute), false).SingleOrDefault();
                if (attr != null)
                {
                    optionMap.Add(attr.Option, new CommandOptionInfo
                    {
                        Attribute = attr,
                        Property = prop,
                    });

                    if (attr.ShortOption != null)
                    {
                        shortOptionMap.Add(attr.ShortOption[0], attr.Option);
                    }
                }
            }

            var args = new Dictionary<string, string>();

            foreach (var arg in commandArgs)
            {
                if (arg.StartsWith("--"))
                {
                    var key = arg.Substring(2);
                    string value = null;

                    var splitIndex = key.IndexOf('=');
                    if (splitIndex != -1)
                    {
                        value = key.Substring(splitIndex + 1);
                        key = key.Substring(0, splitIndex);
                    }

                    args.Add(key, value);
                }
                else if (arg.StartsWith("-"))
                {
                    var key = arg.Substring(1);

                    var splitIndex = key.IndexOf('=');
                    if (splitIndex != -1)
                    {
                        var value = key.Substring(splitIndex + 1);
                        key = key.Substring(0, splitIndex);

                        if (key.Length != 1)
                        {
                            throw new Exception("Invalid short option format");
                        }

                        args.Add(shortOptionMap[key[0]], value);
                    }
                    else
                    {
                        foreach (var c in key)
                        {
                            args.Add(shortOptionMap[c], null);
                        }
                    }
                }
                else
                {
                    throw new Exception($"Unknown argument `{arg}`");
                }
            }

            var requiredOptions = new HashSet<CommandOptionInfo>(optionMap.Values.Where(x => x.Attribute.Required));

            foreach (var arg in args)
            {
                var prop = optionMap[arg.Key];
                requiredOptions.Remove(prop);

                prop.SetValue(instance, arg.Value);
            }

            if (requiredOptions.Any())
            {
                throw new Exception("Required options not specified");
            }
        }
    }

    internal class CommandOptionInfo
    {
        public ConsoleCommandOptionAttribute Attribute { get; set; }
        public PropertyInfo Property { get; set; }

        public void SetValue(object instance, string value)
        {
            var underlyingPropType = Nullable.GetUnderlyingType(Property.PropertyType) ?? Property.PropertyType;

            if (underlyingPropType == typeof(bool))
            {
                Property.SetValue(instance, true);
            }
            else if (underlyingPropType == typeof(string))
            {
                if (value == null)
                {
                    throw new Exception($"Unspecified value for `{Property.Name}`");
                }

                Property.SetValue(instance, value);
            }
            else if (underlyingPropType == typeof(int))
            {
                if (value == null)
                {
                    throw new Exception($"Unspecified value for `{Property.Name}`");
                }

                Property.SetValue(instance, int.Parse(value));
            }
            else if (underlyingPropType == typeof(double))
            {
                if (value == null)
                {
                    throw new Exception($"Unspecified value for `{Property.Name}`");
                }

                Property.SetValue(instance, double.Parse(value));
            }
            else
            {
                throw new Exception($"Unsupported type for `{Property.Name}`");
            }
        }
    }
}
