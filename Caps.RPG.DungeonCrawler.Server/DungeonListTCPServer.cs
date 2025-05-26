using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Caps.RPG.DungeonCrawler.Server
{
    public class DungeonListTCPServer
    {
        private readonly string ConfigDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dungeons");
        private readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.log");

        private string AvailableDungeons => string.Join("|", Directory.GetFiles(ConfigDirectory, "*.xml")
            .Select(Path.GetFileNameWithoutExtension));

        private TcpListener? listener;

        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Log("Server started and listening on port 5000...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Log("Client connected.");
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        private void HandleClient(object? clientObj)
        {
            try
            {
                using TcpClient client = (TcpClient)clientObj!;
                using NetworkStream stream = client.GetStream();

                SendString(stream, AvailableDungeons);

                // Read request from client
                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string requestedName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                Log($"Client requested XML: {requestedName}");

                string filePath = Path.Combine(ConfigDirectory, $"{requestedName}.xml");

                if (File.Exists(filePath))
                {
                    string xml = File.ReadAllText(filePath);
                    SendString(stream, xml);
                }
                else
                {
                    SendString(stream, $"Error: Config '{requestedName}' not found.");
                    Log($"Requested config '{requestedName}' not found.");
                }
            }
            catch (Exception ex)
            {
                LogError("Error handling client", ex);
            }

            Log("Client connection closed.");
        }

        private void SendString(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            stream.Write(data, 0, data.Length);
        }

        private void Log(string message)
        {
            string timestamped = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] INFO: {message}";
            Console.WriteLine(timestamped);
            File.AppendAllText(LogFilePath, timestamped + Environment.NewLine);
        }

        private void LogError(string context, Exception ex)
        {
            string timestamped = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {context}\n{ex}";
            Console.WriteLine(timestamped);
            File.AppendAllText(LogFilePath, timestamped + Environment.NewLine);
        }
    }
}
