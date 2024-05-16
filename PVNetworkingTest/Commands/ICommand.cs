using System.Net.Sockets;

namespace PVNetworkingTest.Commands
{
    public interface ICommand
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract bool Execute(TcpClient client, ArraySegment<string> args, out string response);
    }
}
