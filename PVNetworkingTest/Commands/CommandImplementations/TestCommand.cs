using System.Net.Sockets;

namespace PVNetworkingTest.Commands.CommandImplementations
{
    public class TestCommand : ICommand
    {
        public string Name => "test";

        public string Description => "Zesz";

        public bool Execute(TcpClient client, ArraySegment<string> args, out string response)
        {
            response = args.Count.ToString();
            return true;
        }
    }
}
