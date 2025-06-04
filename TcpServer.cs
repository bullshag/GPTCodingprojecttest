using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TcpServer
{
    public static void Main(string[] args)
    {
        int port = 5000;
        if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
        {
            port = parsedPort;
        }

        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server listening on port {port}");

        while (true)
        {
            Console.WriteLine("Waiting for client...");
            using (var client = listener.AcceptTcpClient())
            using (var stream = client.GetStream())
            {
                Console.WriteLine("Client connected");
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {received}");

                string response = "Echo: " + received;
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
                Console.WriteLine("Response sent");
            }
        }
    }
}
