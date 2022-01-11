﻿using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace Waitingway.Dalamud.Gui;

internal class LoginQueueWindow
{
    private readonly string _version = typeof(Plugin).Assembly.GetName().Version?.ToString() ?? "unknown";
    private readonly PluginUi _ui;
    private int _posX;
    private int _posY;
    private int _width = 400;

    public LoginQueueWindow(PluginUi ui)
    {
        _ui = ui;
    }

    public void Draw()
    {
        if (!_ui.Plugin.InLoginQueue() || _ui.Plugin.GameGui.GetAddonByName("_CharaSelectListMenu", 1) == IntPtr.Zero)
        {
            return;
        }

        ImGui.Begin("Waitingway", ImGuiWindowFlags.NoNav
                                  | ImGuiWindowFlags.NoNavFocus
                                  | ImGuiWindowFlags.NoNavInputs
                                  | ImGuiWindowFlags.NoResize
                                  | ImGuiWindowFlags.NoScrollbar
                                  | ImGuiWindowFlags.NoSavedSettings
                                  | ImGuiWindowFlags.NoFocusOnAppearing
                                  | ImGuiWindowFlags.NoDocking
                                  | ImGuiWindowFlags.NoCollapse
                                  | ImGuiWindowFlags.NoMove
                                  | ImGuiWindowFlags.NoTitleBar
        );
        ImGui.SetWindowPos(new Vector2(_posX, _posY));
        ImGui.SetWindowSize(new Vector2(_width, 0));

        _ui.DrawQueueText();

        ImGui.Separator();
        ImGui.Text($"Waitingway version {_version} (alpha)");
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Globe))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://waitingway.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                PluginLog.LogError($"Failed to open website: {ex}");
            }
        }

        ImGui.TextColored(ImGuiColors.DalamudGrey, "Please report any bugs on the support Discord (the globe icon above).");
        ImGui.End();
    }

    public unsafe void SetDrawPos(AtkUnitBase* loginWindow)
    {
        if (loginWindow == null || loginWindow->RootNode == null)
        {
            return;
        }

        _posX = loginWindow->X;
        _posY = loginWindow->Y + loginWindow->RootNode->Height - 10;
        _width = loginWindow->RootNode->Width;
    }
}