using Microsoft.AspNetCore.SignalR.Client;

namespace NightOwlEnterprise.Api;

using System;
using System.Threading.Tasks;

public class ChatClientService : IAsyncDisposable
{
    private readonly HubConnection _connection;

    public ChatClientService(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .Build();
    }

    public async Task StartAsync()
    {
        _connection.On<string>("ReceiveMessage", (message) =>
        {
            Console.WriteLine($"Message received: {message}");
        });

        await _connection.StartAsync();
        Console.WriteLine("Connection started.");
    }

    public async Task StopAsync()
    {
        await _connection.StopAsync();
        Console.WriteLine("Connection stopped.");
    }
    
    public async Task SendMessageFromSystem(string senderId, string receiverId, string message)
    {
        await _connection.InvokeAsync("SendMessageFromSystem", senderId, receiverId, message);
    }

    public async Task SendSystemMessage(string senderId, string receiverId, string message, SystemMessage systemMessage)
    {
        await _connection.InvokeAsync("SendSystemMessage", senderId, receiverId, message, systemMessage);
    }
    
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
