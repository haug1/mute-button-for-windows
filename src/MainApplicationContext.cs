using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Accessibility;

namespace MuteButton {
  public class MainApplicationContext : ApplicationContext {
    private readonly NotifyIcon _notifyIcon;
    private ToolStripMenuItem _deviceMenu;

    private static Icon CurrentIcon {
      get {
        var suffix = Engine.Instance.IsMicrophoneMuted switch {
          true => "off",
          false => "on",
          _ => "unknown"
        };
        return new Icon($"Icons\\microphone-{suffix}.ico");
      }
    }

    void OnDeviceClicked(object sender, EventArgs e) {
      var deviceItem = (ToolStripMenuItem)sender;
      Engine.Instance.SetDevice(deviceItem.Text);
      UpdateIcon();
    }

    void OnExitClicked(object sender, EventArgs e) {
      _notifyIcon.Visible = false;
      _notifyIcon.Dispose();
      Application.Exit();
    }

    void OnTrayIconDoubleClicked(object sender, EventArgs e) {
      Engine.Instance.ToggleMicrophone();
    }

    void OnMicrophoneToggled(object sender, bool isMuted) {
      UpdateIcon();
    }

    // Recreate every time context menu is opened so that the list is always up-to-date
    void OnContextMenuStripOpening(object sender, EventArgs e) {
      _notifyIcon.ContextMenuStrip.Items.Remove(_deviceMenu);
      _deviceMenu = CreateDeviceMenu();
      _notifyIcon.ContextMenuStrip.Items.Insert(0, _deviceMenu);
    }

    void UpdateIcon() {
      _notifyIcon.Icon = CurrentIcon;
    }

    public MainApplicationContext(Engine engine) {
      // Init system tray icon
      _deviceMenu = CreateDeviceMenu();
      var contextMenuStrip = new ContextMenuStrip() {
        Items = {
            _deviceMenu,
            new ToolStripMenuItem(
              "Exit",
              null,
              OnExitClicked
            )
          }
      };
      contextMenuStrip.Opening += OnContextMenuStripOpening;
      _notifyIcon = new NotifyIcon {
        Icon = CurrentIcon,
        Text = "Mute button",
        ContextMenuStrip = contextMenuStrip,
        Visible = true,
      };
      engine.OnMicrophoneToggled += OnMicrophoneToggled;
      _notifyIcon.DoubleClick += OnTrayIconDoubleClicked;
    }

    ToolStripMenuItem CreateDeviceMenu() {
      var parent = new ToolStripMenuItem("Select device..");
      var deviceItems = MicrophoneControl.ListFriendlyDeviceNames()
        .Select(device => new ToolStripMenuItem(device, null, OnDeviceClicked) {
          Checked = Engine.Instance.SelectedDevice == device
        });
      foreach (var item in deviceItems) {
        parent.DropDownItems.Add(item);
      }
      return parent;
    }
  }
}
