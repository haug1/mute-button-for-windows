using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using NAudio.CoreAudioApi;

namespace MuteButton {

  public delegate void OnMicrophoneToggledEvent(MMDevice microphone, bool isMuted);

  class MicrophoneControl : IDisposable {
    private MMDevice _microphone = null;
    private bool? _lastMutedStatus = null;
    private CancellationTokenSource _soundCancellation;

    public event OnMicrophoneToggledEvent OnMicrophoneToggled;

    public bool? IsMuted {
      get {
        return _microphone?.AudioEndpointVolume.Mute;
      }
    }

    public string SelectedDevice {
      get {
        return _microphone?.DeviceFriendlyName;
      }
    }

    public MicrophoneControl() {
      try {
        var defaultDevice = ListFriendlyDeviceNames().Last();
        SetDevice(defaultDevice);
      } catch {
        Console.WriteLine("WARN: Failed to set microphone on startup.");
      }
    }

    public static IEnumerable<string> ListFriendlyDeviceNames() {
      return _enumerateMicrophoneDevices().Select(device => device.DeviceFriendlyName).ToImmutableList();
    }

    public void SetDevice(string deviceFriendlyName) {
      Dispose();
      _microphone = _enumerateMicrophoneDevices().First(device => device.DeviceFriendlyName == deviceFriendlyName);
      _microphone.AudioEndpointVolume.OnVolumeNotification += OnVolumeNotification;
    }

    public void ToggleMicrophone(bool? forceMute = null) {
      if (_microphone != null)
        _microphone.AudioEndpointVolume.Mute = forceMute.HasValue ? forceMute.Value : !_microphone.AudioEndpointVolume.Mute;
    }

    public void Dispose() {
      if (_microphone != null) {
        ToggleMicrophone(false);
        _microphone.AudioEndpointVolume.OnVolumeNotification -= OnVolumeNotification;
        _microphone.Dispose();
      }
    }

    private static MMDeviceCollection _enumerateMicrophoneDevices() {
      using MMDeviceEnumerator enumerator = new();
      return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
    }

    void OnVolumeNotification(AudioVolumeNotificationData data) {
      if (_lastMutedStatus != data.Muted) {
        _lastMutedStatus = data.Muted;
        Console.WriteLine($"Microphone toggled: IsMuted={data.Muted}");
        OnMicrophoneToggled.Invoke(_microphone, data.Muted);
        _soundCancellation?.Cancel();
        _soundCancellation = new CancellationTokenSource();
        _ = SoundPlayer.PlaySound(
          _microphone.AudioEndpointVolume.Mute ? "mute_button.microphone-muted-teamspeak.mp3" : "mute_button.microphone-activated-teamspeak.mp3",
          _soundCancellation.Token
        );
      }
    }
  }
}
