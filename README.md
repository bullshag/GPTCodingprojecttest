# Tug of War Prototype

This is a small Unity prototype for a network-based two player tug-of-war game. One player hosts and shares their IP address, the other connects using that information. Players tap the screen rapidly during the match to pull the color toward their side.

This project contains a single script `GameLoop.cs` that manages the UI, handles local taps and sends small network messages using `TcpClient`/`TcpListener`.
