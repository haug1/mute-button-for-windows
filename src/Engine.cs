using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MuteButton {
  public class Engine : IDisposable {
    private readonly KeyboardHook _keyboardHook = new();
    private readonly MicrophoneControl _microphoneControl = new();

    public event OnMicrophoneToggledEvent OnMicrophoneToggled;

    private static Engine _instance;

    public static Engine Instance {
      get {
        _instance ??= new();
        return _instance;
      }
    }

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

    private Engine() {
      _keyboardHook.OnKeyUp += OnKeyUp;
      _microphoneControl.OnMicrophoneToggled += (s, e) => OnMicrophoneToggled.Invoke(s, e);
    }

    private void OnKeyUp(Keys key) {
      bool ctrlKeyPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
      bool altKeyPressed = (Control.ModifierKeys & Keys.Alt) == Keys.Alt;
      if (ctrlKeyPressed && altKeyPressed)
        if (key == Keys.M) {
          ToggleMicrophone();
        }
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
  }
}
