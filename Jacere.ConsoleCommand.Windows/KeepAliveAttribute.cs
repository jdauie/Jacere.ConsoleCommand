using System;

namespace Jacere.ConsoleCommand.Windows
{
    [AttributeUsage(AttributeTargets.Class)]
    public class KeepAliveAttribute : Attribute, ICommandPrerequisiteAttribute
    {
        public void Execute()
        {
            if (NativeMethods.SetThreadExecutionState(
                    NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED) == 0)
            {
                throw new Exception("failed to set execution state");
            }
        }
    }
}
