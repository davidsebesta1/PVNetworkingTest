using PVNetworkingTest.Commands;

namespace PVNetworkingTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            CommandHandler.RegisterAllCommands();

            ConnectionsHandler.Instance.TryOpen();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
