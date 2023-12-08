using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace mute_button {
  class Engine : IDisposable {
    private readonly KeyboardHook _keyboardHook;
    private readonly MicrophoneControl _microphoneControl = new("Trust USB microphone");

    public Engine() {
      _keyboardHook = new(OnKeyUp);
    }


    private void OnKeyUp(Keys key) {
      bool ctrlKeyPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
      bool altKeyPressed = (Control.ModifierKeys & Keys.Alt) == Keys.Alt;
      if (ctrlKeyPressed && altKeyPressed && key == Keys.M) {
        var isMuted = _microphoneControl.ToggleMicrophone();
        _ = playSound(isMuted);
      }
    }

    private static async Task playSound(bool micMuted) {
      string resourceName = micMuted ? "mute_button.microphone-muted-teamspeak.mp3" : "mute_button.microphone-activated-teamspeak.mp3";
      Assembly assembly = Assembly.GetExecutingAssembly();
      using Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
      using var waveOut = new WaveOutEvent();
      using var reader = new Mp3FileReader(resourceStream);
      waveOut.Init(reader);
      waveOut.Volume = 0.05f;
      waveOut.Play();
      while (waveOut.PlaybackState == PlaybackState.Playing) {
        await Task.Delay(100);
      }
    }

    public void Dispose() {
      _keyboardHook.Dispose();
      _microphoneControl.Dispose();
      Console.WriteLine("Disposed successfully.");
    }
  }
}
