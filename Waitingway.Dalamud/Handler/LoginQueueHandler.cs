﻿using System;
using Dalamud.Logging;
using Waitingway.Common.Protocol.Serverbound;
using Waitingway.Dalamud.Network;

namespace Waitingway.Dalamud.Handler;

internal class LoginQueueHandler : IDisposable
{
    private Plugin Plugin { get; }
    private WaitingwayClient Client => Plugin.Client;

    public LoginQueueHandler(Plugin plugin)
    {
        Plugin = plugin;
        plugin.ClientState.Login += HandleLogin;
    }

    private void HandleLogin(object? sender, EventArgs eventArgs)
    {
        PluginLog.Debug($"HandleLogin: sender = {sender}, eventArgs = {eventArgs}");
        if (Client.InQueue)
        {
            var duration = DateTime.Now - Client.QueueEntryTime;
            PluginLog.Log($"Login queue took {duration}");
            PluginLog.Log($"Login queue took {duration}");
            Client.ExitQueue(QueueExit.QueueExitReason.Success);
        }

        // we don't want to hook yesno dialogs ingame
        Plugin.Hooks.ToggleSelectYesNoHook(false);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Plugin.ClientState.Login -= HandleLogin;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}