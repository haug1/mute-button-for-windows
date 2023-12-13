using NAudio.CoreAudioApi;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MuteButton.Audio {

  public delegate void OnMicrophoneToggledEvent(MMDevice microphone, bool isMuted);

  class MicrophoneControl : IDisposable {
    private MMDevice? _microphone = null;
    private bool? _lastMutedStatus = null;
    private CancellationTokenSource? _soundCancellation = null;

    public event OnMicrophoneToggledEvent? OnMicrophoneToggled = null;

    public bool? IsMuted {
      get {
        return _microphone?.AudioEndpointVolume.Mute;
      }
    }

    public string? SelectedDevice {
      get {
        return _microphone?.DeviceFriendlyName;
      }
    }

    public MicrophoneControl() {
      try {
        var defaultDevice = ListFriendlyDeviceNames().Last();
        SetDevice(defaultDevice);
      } catch {
        Debug.WriteLine("WARN: Failed to set microphone on startup.");
      }
    }

    public static IEnumerable<string> ListFriendlyDeviceNames() {
      return _enumerateMicrophoneDevices().Select(device => device.DeviceFriendlyName).ToImmutableList();
    }

    public void SetDevice(string deviceFriendlyName) {
      Dispose();
      _microphone = _enumerateMicrophoneDevices().First(device => device.DeviceFriendlyName == deviceFriendlyName);
      _microphone.AudioEndpointVolume.OnVolumeNotification += _onVolumeNotification;
    }

    public void ToggleMicrophone(bool? forceMute = null) {
      if (_microphone != null)
        _microphone.AudioEndpointVolume.Mute = forceMute.HasValue ? forceMute.Value : !_microphone.AudioEndpointVolume.Mute;
    }

    public void Dispose() {
      if (_microphone != null) {
        ToggleMicrophone(false);
        _microphone.AudioEndpointVolume.OnVolumeNotification -= _onVolumeNotification;
        _microphone.Dispose();
      }
    }

    private static MMDeviceCollection _enumerateMicrophoneDevices() {
      using MMDeviceEnumerator enumerator = new();
      return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
    }

    void _onVolumeNotification(AudioVolumeNotificationData data) {
      if (_lastMutedStatus != data.Muted) {
        _lastMutedStatus = data.Muted;
        Debug.WriteLine($"Microphone toggled: IsMuted={data.Muted}");
        OnMicrophoneToggled?.Invoke(_microphone!, data.Muted);
        _soundCancellation?.Cancel();
        _soundCancellation = new CancellationTokenSource();

        var micStatus = _microphone!.AudioEndpointVolume.Mute ? Properties.Resources.microphone_muted_teamspeak : Properties.Resources.microphone_activated_teamspeak;
        _ = SoundPlayer.PlaySound(
           micStatus,
          _soundCancellation.Token
        );
      }
    }
  }
}
