using System.Net.Sockets;
using System.Reflection;

namespace PVNetworkingTest.Commands
{
    public static class CommandHandler
    {
        private static readonly Dictionary<string, ICommand> _registeredCommands = new Dictionary<string, ICommand>();

        public static bool TryExecuteCommand(TcpClient client, string commandName, ArraySegment<string> args, out string response)
        {
            if (_registeredCommands == null)
            {
                response = "Command not found";
                return false;
            }

            if (_registeredCommands.TryGetValue(commandName, out ICommand command))
            {
                return command.Execute(client, args, out response);
            }

            response = "Command not found";
            return false;
        }

        private static bool TryRegisterCommand(Type type)
        {
            if (Activator.CreateInstance(type) is not ICommand command)
            {
                return false;
            }

            _registeredCommands.Add(command.Name, command);
            Console.WriteLine("Registering command " + type.Name);
            return true;
        }

        public static bool TryRegisterCommand<T>() where T : ICommand
        {
            return TryRegisterCommand(typeof(T));
        }

        public static void RegisterAllCommands()
        {
            var cmds = Assembly.GetCallingAssembly().GetTypes().Where(n => n.GetInterface("ICommand") != null);

            foreach (Type? cmd in cmds)
            {
                TryRegisterCommand(cmd);
            }
        }
    }
}
