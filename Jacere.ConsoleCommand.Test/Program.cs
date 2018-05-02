using System.Reflection;
using System.Threading.Tasks;

namespace Jacere.ConsoleCommand.Test
{
    class Program
    {
        public static async Task Main()
        {
            await ConsoleCommandDispatcher.DispatchCommand(new[]{Assembly.GetEntryAssembly()}, description: @"
                here be some description
            ");
        }
    }
}
