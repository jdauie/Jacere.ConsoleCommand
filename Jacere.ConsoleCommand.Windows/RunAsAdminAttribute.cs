using System;
using System.Security.Principal;

namespace Jacere.ConsoleCommand.Windows
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RunAsAdminAttribute : Attribute, ICommandPrerequisiteAttribute
    {
        public void Execute()
        {
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                throw new Exception("this command must be run as administrator");
            }
        }
    }
}
