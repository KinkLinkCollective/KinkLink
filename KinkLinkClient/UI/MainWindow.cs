using System.Numerics;
using KinkLinkClient.Domain;
using KinkLinkClient.Domain.Interfaces;
using KinkLinkClient.Services;
using KinkLinkClient.UI.Components.NavigationBar;
using KinkLinkClient.UI.Views.CustomizePlus;
using KinkLinkClient.UI.Views.Debug;
using KinkLinkClient.UI.Views.Emote;
using KinkLinkClient.UI.Views.Friends;
using KinkLinkClient.UI.Views.History;
using KinkLinkClient.UI.Views.Honorific;
using KinkLinkClient.UI.Views.Login;
using KinkLinkClient.UI.Views.Moodles;
using KinkLinkClient.UI.Views.Pause;
using KinkLinkClient.UI.Views.Settings;
using KinkLinkClient.UI.Views.Speak;
using KinkLinkClient.UI.Views.Status;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace KinkLinkClient.UI;

public class MainWindow : Window
{
    // Const
    private static readonly string MainWindowTitle = $"Aether Remote 2 - Version {Plugin.Version}";

    // Services
    private readonly ViewService _viewService;

    // Components
    private readonly NavigationBarComponentUi _navigationBar;

    // Views
    private readonly CustomizePlusViewUi _customizePlusView;
    private readonly DebugViewUi _debugView;
    private readonly EmoteViewUi _emoteView;
    private readonly FriendsViewUi _friendsView;
    private readonly HistoryViewUi _historyView;
    private readonly HonorificViewUi _honorificView;
    private readonly LoginViewUi _loginView;
    private readonly MoodlesViewUi _moodlesView;
    private readonly PauseViewUi _pauseView;
    private readonly SettingsViewUi _settingsView;
    private readonly SpeakViewUi _speakView;
    private readonly StatusViewUi _statusView;

    public MainWindow(
        ViewService viewService,
        NavigationBarComponentUi navigationBarComponentUi,
        CustomizePlusViewUi customizePlusView,
        DebugViewUi debugView,
        EmoteViewUi emoteView,
        FriendsViewUi friendsView,
        HistoryViewUi historyView,
        HonorificViewUi honorificView,
        LoginViewUi loginView,
        MoodlesViewUi moodlesView,
        PauseViewUi pauseView,
        SettingsViewUi settingsView,
        SpeakViewUi speakView,
        StatusViewUi statusView) : base(MainWindowTitle)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _viewService = viewService;

        _navigationBar = navigationBarComponentUi;

        _customizePlusView = customizePlusView;
        _debugView = debugView;
        _emoteView = emoteView;
        _friendsView = friendsView;
        _historyView = historyView;
        _honorificView = honorificView;
        _loginView = loginView;
        _moodlesView = moodlesView;
        _pauseView = pauseView;
        _settingsView = settingsView;
        _speakView = speakView;
        _statusView = statusView;
    }

    public override void Draw()
    {
        _navigationBar.Draw();

        ImGui.SameLine();

        IDrawable view = _viewService.CurrentView switch
        {
            View.CustomizePlus => _customizePlusView,
            View.Debug => _debugView,
            View.Emote => _emoteView,
            View.Friends => _friendsView,
            View.History => _historyView,
            View.Honorific => _honorificView,
            View.Login => _loginView,
            View.Moodles => _moodlesView,
            View.Pause => _pauseView,
            View.Settings => _settingsView,
            View.Speak => _speakView,
            View.Status => _statusView,
            _ => _loginView
        };

        view.Draw();
    }
}
