using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Jacere.ConsoleCommand.Windows
{
    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern uint SetThreadExecutionState(uint esFlags);

        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;
    }
}