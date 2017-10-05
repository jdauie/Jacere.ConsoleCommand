using System.Threading.Tasks;

namespace Jacere.ConsoleCommand
{
    public interface IConsoleCommand
    {
        Task Execute();
    }
}
