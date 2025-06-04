using System;
using System.Net.Sockets;
using System.Text;

public class TcpClientExample
{
    public static void Main(string[] args)
    {
        string host = "127.0.0.1";
        int port = 5000;
        string message = "Hello";

        if (args.Length > 0)
            host = args[0];
        if (args.Length > 1 && int.TryParse(args[1], out int parsedPort))
            port = parsedPort;
        if (args.Length > 2)
            message = args[2];

        using (var client = new TcpClient())
        {
            client.Connect(host, port);
            using (var stream = client.GetStream())
            {
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(msgBytes, 0, msgBytes.Length);

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Server replied: {response}");
            }
        }
    }
}
