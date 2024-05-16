using PVNetworkingTest.Commands;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PVNetworkingTest
{
    public class ConnectionsHandler
    {
        private static ConnectionsHandler _instance;
        public static ConnectionsHandler? Instance
        {
            get
            {
                return _instance ??= new ConnectionsHandler();
            }
        }

        private TcpListener? _listener;
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        private readonly List<TcpClient> _toBeDisconnected = new List<TcpClient>();

        private readonly object _clientLock = new object();

        private ConnectionsHandler() { }

        public void TryOpen()
        {
            Console.WriteLine("Trying to open connection");
            if (_listener == null)
            {
                Console.WriteLine("Opening connection");
                Thread threadServer = new Thread(ServerThread);
                threadServer.Start();
            }
        }

        public async Task SendDataToClientAsync(TcpClient client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await SendDataToClientAsync(client, data);
        }

        public async Task SendDataToClientAsync(TcpClient client, byte[] data)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to client: {ex.Message}");

                lock (_clientLock)
                {
                    _connectedClients.Remove(client);
                }
            }
        }

        public async Task SendDataToAllClientsAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            Console.WriteLine("Sending: " + message);
            foreach (TcpClient client in _connectedClients)
            {
                await SendDataToClientAsync(client, data);
            }

            lock (_clientLock)
            {
                foreach (TcpClient client in _toBeDisconnected)
                {
                    _connectedClients.Remove(client);
                }
            }
        }

        private async void ServerThread()
        {
            Console.WriteLine("Starting new thread for the connection");
            try
            {
                IPAddress ipAddress = IPAddress.Any;
                int port = 8000;

                _listener = new TcpListener(ipAddress, port);
                _listener.Start();

                Console.WriteLine($"Server is listening on port {port}...");

                while (true)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    lock (_clientLock)
                    {
                        _connectedClients.Add(client);
                    }

                    await Task.Run(async () => await HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
                    {
                        string data = (Encoding.UTF8.GetString(buffer, 0, bytesRead)).Trim();

                        if (string.IsNullOrEmpty(data)) continue;

                        Console.WriteLine("Received: " + data);
                        await HandleReceivedData(client, data);
                    }

                    lock (_clientLock)
                    {
                        _connectedClients.Remove(client);
                        Console.WriteLine("Disconnecting client, stream ended");
                    }
                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionAborted })
            {
                Console.WriteLine("Client disconnected. " + ex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleReceivedData(TcpClient client, string data)
        {
            string[] splits = data.Split(' ');
            ArraySegment<string> args = splits.Length > 1 ? new ArraySegment<string>(splits, 1, splits.Length - 1) : new ArraySegment<string>();
            CommandHandler.TryExecuteCommand(client, splits[0], args, out string response);

            await SendDataToClientAsync(client, response);
        }
    }
}
