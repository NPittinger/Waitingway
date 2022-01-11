﻿using System;
using System.Threading.Tasks;
using Dalamud.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using Waitingway.Common.Protocol;
using Waitingway.Common.Protocol.Clientbound;
using Waitingway.Common.Protocol.Serverbound;

namespace Waitingway.Dalamud.Network;

public class WaitingwayClient : IAsyncDisposable
{
    private readonly Plugin _plugin;
    private readonly string _clientId;
    private readonly HubConnection _connection;
    private string _language;
    private bool _connected;
    private bool _gotGoodbye;

    public WaitingwayClient(Plugin plugin, string serverUrl, string clientId, string language)
    {
        _plugin = plugin;
        _clientId = clientId;
        _connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .WithAutomaticReconnect()
            .Build();
        _language = language;

        RegisterHandlers();

        plugin.Ui.SetStatusText("Waiting for server...");
        Task.Run(async () =>
        {
            PluginLog.Log($"Attempting to connect to remote server at {serverUrl}.");
            await _connection.StartAsync();
            _gotGoodbye = false;
            _connected = true;
            PluginLog.Log("Connected to server.");
            await SendHello();

            // register the reconnect handler in case we get disconnected
            _connection.Reconnected += OnReconnect;
            _connection.Closed += OnDisconnect;
        });
    }

    private void RegisterHandlers()
    {
        // this kinda sucks, can we do better with attributes + reflection or something?
        _connection.On<ServerHello>(nameof(ServerHello), HandleServerHello);
        _connection.On<ServerGoodbye>(nameof(ServerGoodbye), HandleServerGoodbye);
        _connection.On<QueueStatusEstimate>(nameof(QueueStatusEstimate), HandleQueueStatusEstimate);
    }

    private void HandleServerHello(ServerHello packet)
    {
#if DEBUG
        PluginLog.LogDebug("Received ServerHello packet");
#endif
    }

    private void HandleServerGoodbye(ServerGoodbye packet)
    {
#if DEBUG
        PluginLog.LogDebug("Received ServerGoodbye packet");
#endif
        _gotGoodbye = true;
        if (packet.Message != null)
        {
            _plugin.Ui.SetStatusText(packet.Message);
        }
    }

    private void HandleQueueStatusEstimate(QueueStatusEstimate packet)
    {
#if DEBUG
        PluginLog.LogDebug("Received QueueStatusEstimate packet");
#endif
        _plugin.Ui.QueueText = packet.LocalisedMessages;
    }

    private async Task OnReconnect(string? _)
    {
        _gotGoodbye = false;
        _connected = true;
        PluginLog.Log("Reconnected to server.");
        await SendHello();
    }

    private Task OnDisconnect(Exception? ex)
    {
        _connected = false;
        PluginLog.Log($"Disconnected from server. {ex}");

        if (!_gotGoodbye)
        {
            _plugin.Ui.SetStatusText("Disconnected from server unexpectedly.\nCheck Dalamud logs for more information.");
        }

        return Task.CompletedTask;
    }

    private async Task SendHello()
    {
        await Send(new ClientHello
        {
            ProtocolVersion = 1,
            ClientId = _clientId,
            PluginVersion = typeof(Plugin).Assembly.GetName().Version?.ToString() ?? "unknown",
            Language = _language
        });
    }

    internal async void LanguageChanged(string newLanguage)
    {
        _language = newLanguage;
        await Send(new ClientLanguageChange {Language = newLanguage});
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _connection.DisposeAsync();
    }

    public async Task Send(IPacket packet)
    {
        if (!_connected)
        {
            PluginLog.Warning($"Not connected to server, skipping send of {packet.GetType().Name} packet");
            return;
        }

        try
        {
#if DEBUG
            PluginLog.LogDebug($"Sending {packet.GetType().Name} packet");
#endif
            await _connection.InvokeAsync(packet.GetType().Name, packet);
        }
        catch (Exception ex)
        {
            PluginLog.Log($"Error while sending packet to server: {ex}");
        }
    }
}