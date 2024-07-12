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

    public async Task SendInvitationSpecifiedHourMessage(string senderId, string receiverId, string message, InvitationSpecifiedHourMessage invitationSpecifiedHourMessage)
    {
        await _connection.InvokeAsync("SendInvitationSpecifiedHourMessage", senderId, receiverId, message, invitationSpecifiedHourMessage);
    }
    
    public async Task SendInvitationApprovedMessage(string senderId, string receiverId, string message, InvitationApprovedMessage invitationApprovedMessage)
    {
        await _connection.InvokeAsync("SendInvitationApprovedMessage", senderId, receiverId, message, invitationApprovedMessage);
    }
    
    public async Task SendInvitationCancelledMessage(string senderId, string receiverId, string message, InvitationCancelledMessage invitationCancelledMessage)
    {
        await _connection.InvokeAsync("SendInvitationCancelledMessage", senderId, receiverId, message, invitationCancelledMessage);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
