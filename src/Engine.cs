using System;
using System.Windows.Forms;
using MuteButton.Audio;
using MuteButton.Hooks;

namespace MuteButton {
  public class Engine : IDisposable {
    private readonly KeyboardHook _keyboardHook = new();
    private readonly MicrophoneControl _microphoneControl = new();

    public event OnMicrophoneToggledEvent OnMicrophoneToggled;

    public bool? IsMicrophoneMuted {
      get {
        return _microphoneControl.IsMuted;
      }
    }

    public string SelectedDevice {
      get {
        return _microphoneControl.SelectedDevice;
      }
    }

    public Engine() {
      _keyboardHook.OnKeyUp += _onKeyUp;
      _microphoneControl.OnMicrophoneToggled += (s, e) => OnMicrophoneToggled.Invoke(s, e);
    }

    public void ToggleMicrophone() {
      _microphoneControl.ToggleMicrophone();
    }

    public void SetDevice(string device) {
      _microphoneControl.SetDevice(device);
    }

    public void Dispose() {
      _keyboardHook.Dispose();
      _microphoneControl.Dispose();
      Console.WriteLine("Disposed successfully.");
    }

    private void _onKeyUp(Keys key) {
      bool ctrlKeyPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
      bool altKeyPressed = (Control.ModifierKeys & Keys.Alt) == Keys.Alt;
      if (ctrlKeyPressed && altKeyPressed)
        if (key == Keys.M) {
          ToggleMicrophone();
        }
    }
  }
}
