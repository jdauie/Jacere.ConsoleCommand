using System;
using System.Threading.Tasks;

namespace Jacere.ConsoleCommand.Test.Commands
{
    [ConsoleCommand("Test4ThisIsAMuchLongerName", "another description")]
    public class Test4Command : IConsoleCommand
    {
        [ConsoleCommandOption("arg1", "a", true, "short description")]
        public bool Arg1 { get; set; }

        [ConsoleCommandOption("arg2", "b")]
        public int Arg2 { get; set; }

        [ConsoleCommandOption("arg3", required: true)]
        public double Arg3 { get; set; }

        [ConsoleCommandOption("arg4")]
        public Guid Arg4 { get; set; }

        [ConsoleCommandOption("arg5", "s", description: @"
            long description
            on multiple lines
                with indenting
            and stuff
        ")]
        public string Arg5 { get; set; }

        [ConsoleCommandOption("arg6")]
        public static bool Arg6 { get; set; }

        public Task Execute()
        {
            return Task.CompletedTask;
        }
    }
}
