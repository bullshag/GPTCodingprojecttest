# Tug of War Prototype

This is a small Unity prototype for a network-based two player tug-of-war game.
One player hosts and shares their IP address, the other connects using that information.
Players tap the screen rapidly during the match to pull the color toward their side.

This project originally contained a single script `GameLoop.cs` that manages the UI,
handles local taps and sends small network messages using `TcpClient`/`TcpListener`.

Additional standalone C# examples have been added in `TcpServer.cs` and
`TcpClientExample.cs`. They demonstrate basic TCP server and client usage from a
console application.
