using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

    public class ConsoleCommandDispatcher
    {
        private const int CommandIndent = 2;
        private const int DescriptionIndent = 4;

        public static async Task DispatchCommand(IEnumerable<Assembly> assemblies, string commandName = null, string description = null)
        {
            await DispatchCommand(assemblies.SelectMany(x => x.GetTypes()), commandName, description);
        }
        
        private static async Task DispatchCommand(IEnumerable<Type> types, string commandName, string description = null)
        {
            var commands = types
                .Where(x => typeof(IConsoleCommand).IsAssignableFrom(x))
                .Where(x => x.IsClass && !x.IsAbstract)
                .Select(x => new CommandInfo
                {
                    Type = x,
                    Constructor = x.GetConstructor(new Type[0]),
                    Attribute = (ConsoleCommandAttribute)x.GetCustomAttributes(typeof(ConsoleCommandAttribute), false).SingleOrDefault()
                })
                .Where(x => x.Attribute != null && x.Constructor != null)
                .ToList();

            if (!commands.Any())
            {
                throw new Exception($"There are no valid commands. Commands must implement {nameof(IConsoleCommand)}, have a default constructor, and have a single {nameof(ConsoleCommandAttribute)}.");
            }
            
            var duplicateCommands = commands.GroupBy(x => x.Name)
                .Select(x => x.ToList())
                .Where(x => x.Count > 1)
                .ToList();

            if (duplicateCommands.Any())
            {
                throw new Exception($"Duplicate command names: {string.Join(", ", duplicateCommands.Select(x => x.First().Name))}");
            }

            var args = Environment.GetCommandLineArgs().Skip(1).ToList();

            if (commandName == null)
            {
                commandName = args.FirstOrDefault();
                args = args.Skip(1).ToList();
            }

            var commandInfo = commands
                .SingleOrDefault(x => string.Equals(x.Name, commandName, StringComparison.OrdinalIgnoreCase));

            if (commandInfo == null || commandName == "help")
            {
                PrintCommands(description, commands);
                return;
            }

            var command = commandInfo.Create();

            try
            {
                BindCommandArgs(command.GetType(), command, args);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);

                PrintUsage(commandInfo);
                return;
            }

            await command.Execute();
        }

        private static void PrintUsage(CommandInfo command)
        {
            var options = GetOptions(command.Type);
            
            Console.WriteLine();

            if (!string.IsNullOrEmpty(command.Attribute.LongDescription))
            {
                Console.WriteLine(GetDescriptionFromIndentedVerbatimLiteral(command.Attribute.LongDescription));
                Console.WriteLine();
            }

            Console.WriteLine($"Expected usage: {command.Name} <options>");
            Console.WriteLine("<options> available:");
            Console.WriteLine();

            var lines = new List<List<string>>();

            foreach (var option in options)
            {
                var optionValue = option.IsValueType
                    ? "=VALUE"
                    : "";

                lines.Add(new List<string>
                {
                    string.IsNullOrEmpty(option.Attribute.ShortOption) ? "" : $"-{option.Attribute.ShortOption}",
                    $"--{option.Attribute.Option}{optionValue}",
                    GetDescriptionFromIndentedVerbatimLiteral(option.Attribute.Description),
                });
            }

            PrintLines(lines, CommandIndent, CommandIndent, DescriptionIndent);
        }

        private static void PrintCommands(string description, ICollection<CommandInfo> commands)
        {
            Console.WriteLine();

            if (!string.IsNullOrEmpty(description))
            {
                Console.WriteLine(GetDescriptionFromIndentedVerbatimLiteral(description));
                Console.WriteLine();
            }

            Console.WriteLine("Available commands:");
            Console.WriteLine();

            var lines = new List<List<string>>();

            foreach (var command in commands)
            {
                lines.Add(new List<string>
                {
                    command.Name,
                    GetDescriptionFromIndentedVerbatimLiteral(command.Attribute.ShortDescription),
                });
            }

            lines.Add(null);
            lines.Add(new List<string>
            {
                "help <name>",
                "For help with one of the above commands",
            });

            PrintLines(lines, CommandIndent, DescriptionIndent);
            
            Console.WriteLine();
        }

        private static void PrintLines(IReadOnlyCollection<IList<string>> lines, params int[] pads)
        {
            var maxLengths = lines.First()
                .Select((_, i) => lines.Max(x => x?[i]?.Length ?? 0))
                .ToList();

            foreach (var line in lines)
            {
                var lineQueues = (line ?? lines.First().Select(_ => ""))
                    .Select(x => new Queue<string>((x ?? "").Split('\n')))
                    .ToList();

                while (lineQueues.Any(x => x.Any()))
                {
                    for (var i = 0; i < lineQueues.Count; i++)
                    {
                        var queue = lineQueues[i];
                        var maxLength = maxLengths[i];

                        var value = queue.Any()
                            ? queue.Dequeue()
                            : "";

                        if (pads.Length > i)
                        {
                            Console.Write("".PadRight(pads[i]));
                        }

                        Console.Write(value.PadRight(maxLength));
                    }
                    Console.WriteLine();
                }
            }
        }

        private static string GetDescriptionFromIndentedVerbatimLiteral(string description)
        {
            if (description == null)
            {
                description = "";
            }

            var lines = description.Split('\n');

            if (lines.Length > 1)
            {
                var firstInterestingLine = lines.First(x => x.Trim().Length != 0);
                var indent = firstInterestingLine.Substring(0, firstInterestingLine.Length - firstInterestingLine.TrimStart().Length);

                description = string.Join("\n", lines
                    .Select(x => x.StartsWith(indent) ? x.Substring(indent.Length) : x))
                    .Trim();
            }
            else
            {
                description = description.Trim();
            }

            return description.Replace("\r", "");
        }

        private static void BindCommandArgs(Type target, object instance, IEnumerable<string> commandArgs)
        {
            var optionMap = new Dictionary<string, CommandOptionInfo>();
            var shortOptionMap = new Dictionary<char, string>();

            var options = GetOptions(target);

            foreach (var option in options)
            {
                optionMap.Add(option.Attribute.Option, option);

                if (option.Attribute.ShortOption != null)
                {
                    shortOptionMap.Add(option.Attribute.ShortOption[0], option.Attribute.Option);
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
                            throw new Exception($"Invalid short option format: `{arg}`");
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

        private static ICollection<CommandOptionInfo> GetOptions(Type target)
        {
            var props = target.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var options = new List<CommandOptionInfo>();

            foreach (var prop in props)
            {
                var attr = (ConsoleCommandOptionAttribute) prop
                    .GetCustomAttributes(typeof(ConsoleCommandOptionAttribute), false).SingleOrDefault();
                if (attr != null)
                {
                    options.Add(new CommandOptionInfo
                    {
                        Attribute = attr,
                        Property = prop,
                    });
                }
            }

            return options;
        }
    }

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
