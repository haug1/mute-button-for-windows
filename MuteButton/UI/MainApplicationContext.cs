using MuteButton.Audio;

namespace MuteButton.UI {
  public class MainApplicationContext : ApplicationContext {
    private readonly NotifyIcon _notifyIcon;
    private readonly Engine _engine;

    private ToolStripMenuItem _deviceMenu;

    private Icon CurrentIcon {
      get {
        return _engine.IsMicrophoneMuted switch {
          true => Properties.Resources.microphone_off,
          false => Properties.Resources.microphone_on,
          _ => Properties.Resources.microphone_unknown
        };
      }
    }

    void _onDeviceClicked(object? sender, EventArgs e) {
      if (sender == null) return;
      var deviceItem = (ToolStripMenuItem)sender;
      _engine.SetDevice(deviceItem.Text!);
      _updateIcon();
    }

    void _onExitClicked(object? sender, EventArgs e) {
      _notifyIcon.Visible = false;
      _notifyIcon.Dispose();
      Application.Exit();
    }

    void _onTrayIconDoubleClicked(object? sender, EventArgs e) {
      _engine.ToggleMicrophone();
    }

    void _onMicrophoneToggled(object? sender, bool isMuted) {
      _updateIcon();
    }

    // Recreate every time context menu is opened so that the list is always up-to-date
    void _onContextMenuStripOpening(object? sender, EventArgs e) {
      _notifyIcon?.ContextMenuStrip?.Items.Remove(_deviceMenu);
      _deviceMenu = _createDeviceMenu();
      _notifyIcon?.ContextMenuStrip?.Items.Insert(0, _deviceMenu);
    }

    void _updateIcon() {
      _notifyIcon.Icon = CurrentIcon;
    }

    public MainApplicationContext(Engine engine) {
      _engine = engine;
      // Init system tray icon
      _deviceMenu = _createDeviceMenu();
      var contextMenuStrip = new ContextMenuStrip() {
        Items = {
            _deviceMenu,
            new ToolStripMenuItem(
              "Exit",
              null,
              _onExitClicked
            )
          }
      };
      contextMenuStrip.Opening += _onContextMenuStripOpening;
      _notifyIcon = new NotifyIcon {
        Icon = CurrentIcon,
        Text = "Mute button",
        ContextMenuStrip = contextMenuStrip,
        Visible = true,
      };
      engine.OnMicrophoneToggled += _onMicrophoneToggled;
      _notifyIcon.DoubleClick += _onTrayIconDoubleClicked;
    }

    ToolStripMenuItem _createDeviceMenu() {
      var parent = new ToolStripMenuItem("Select device..");
      var items = MicrophoneControl.ListFriendlyDeviceNames()
        .Select(deviceName => new ToolStripMenuItem(deviceName, image: null, _onDeviceClicked) {
          Checked = _engine.SelectedDevice == deviceName
        });
      foreach (var item in items) {
        parent.DropDownItems.Add(item);
      }
      return parent;
    }
  }
}
