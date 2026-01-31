using System;
using KinkLinkClient.Style;
using KinkLinkClient.UI;
using KinkLinkClient.Utils;
using Dalamud.Interface.Windowing;

namespace KinkLinkClient.Managers;

public class WindowManager : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly WindowSystem _windowSystem;

    public WindowManager(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        _windowSystem = new WindowSystem("Kink Link");
        _windowSystem.AddWindow(mainWindow);

#if DEBUG
        _mainWindow.IsOpen = true;
#endif

        Plugin.PluginInterface.UiBuilder.Draw += Draw;
        Plugin.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        Plugin.PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;
    }

    private void Draw()
    {
        KinkLinkImGui.Push();
        _windowSystem.Draw();
        KinkLinkImGui.Pop();
    }

    private void OpenMainUi()
    {
        _mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        Plugin.PluginInterface.UiBuilder.Draw -= Draw;
        Plugin.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        Plugin.PluginInterface.UiBuilder.OpenConfigUi -= OpenMainUi;

        _windowSystem.RemoveAllWindows();

        GC.SuppressFinalize(this);
    }
}
