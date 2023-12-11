using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MuteButton.Audio;

namespace MuteButton.UI {
  public class MainApplicationContext : ApplicationContext {
    private readonly NotifyIcon _notifyIcon;
    private readonly Engine _engine;
    private ToolStripMenuItem _deviceMenu;

    private Icon CurrentIcon {
      get {
        var suffix = _engine.IsMicrophoneMuted switch {
          true => "off",
          false => "on",
          _ => "unknown"
        };
        return new Icon($"Resources/Icons/microphone-{suffix}.ico");
      }
    }

    void _onDeviceClicked(object sender, EventArgs e) {
      var deviceItem = (ToolStripMenuItem)sender;
      _engine.SetDevice(deviceItem.Text);
      _updateIcon();
    }

    void _onExitClicked(object sender, EventArgs e) {
      _notifyIcon.Visible = false;
      _notifyIcon.Dispose();
      Application.Exit();
    }

    void _onTrayIconDoubleClicked(object sender, EventArgs e) {
      _engine.ToggleMicrophone();
    }

    void _onMicrophoneToggled(object sender, bool isMuted) {
      _updateIcon();
    }

    // Recreate every time context menu is opened so that the list is always up-to-date
    void _onContextMenuStripOpening(object sender, EventArgs e) {
      _notifyIcon.ContextMenuStrip.Items.Remove(_deviceMenu);
      _deviceMenu = _createDeviceMenu();
      _notifyIcon.ContextMenuStrip.Items.Insert(0, _deviceMenu);
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
      var deviceItems = MicrophoneControl.ListFriendlyDeviceNames()
        .Select(device => new ToolStripMenuItem(device, null, _onDeviceClicked) {
          Checked = _engine.SelectedDevice == device
        });
      foreach (var item in deviceItems) {
        parent.DropDownItems.Add(item);
      }
      return parent;
    }
  }
}
