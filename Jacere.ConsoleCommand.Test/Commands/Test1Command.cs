﻿using System;
using System.Threading.Tasks;
using Jacere.ConsoleCommand.Windows;

namespace Jacere.ConsoleCommand.Test.Commands
{
    [ConsoleCommand("Test1")]
    [KeepAlive]
    [RunAsAdmin]
    public class Test1Command : IConsoleCommand
    {
        [ConsoleCommandOption("arg1", "a", true, "short description")]
        public bool Arg1 { get; set; }

        [ConsoleCommandOption("arg2-a-relatively-long-command", "b")]
        public int Arg2 { get; set; }

        [ConsoleCommandOption("arg3", required: true)]
        public double Arg3 { get; set; }

        [ConsoleCommandOption("arg4-medium")]
        public Guid Arg4 { get; set; }

        [ConsoleCommandOption("arg5", "s", description: @"
            long description
            on multiple lines
                with indenting
            and stuff
        ")]
        public string Arg5 { get; set; }

        public Task Execute()
        {
            return Task.CompletedTask;
        }
    }
}
