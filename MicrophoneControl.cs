using System;
using NAudio.CoreAudioApi;

namespace mute_button {
  class MicrophoneControl : IDisposable {
    private readonly MMDeviceEnumerator _enumerator;
    private readonly MMDevice _microphone;

    public MicrophoneControl(string micName) {
      _enumerator = new MMDeviceEnumerator();
      MMDeviceCollection devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);

      for (int i = 0; i < devices.Count; i++) {
        var device = devices[i];
        Console.WriteLine($"Device {i}");
        Console.WriteLine($"DeviceFriendlyName: {device.DeviceFriendlyName}");
        Console.WriteLine($"FriendlyName: {device.FriendlyName}");
        Console.WriteLine();

        if (device.DeviceFriendlyName.Contains(micName)) {
          Console.WriteLine($"Selected device: {device.DeviceFriendlyName} {{{device.ID}}}");
          _microphone = device;
          break;
        }
      }

      // We must interact with the device for the handle to register. 
      LogMicrophoneStatus();
    }

    public void Dispose() {
      ToggleMicrophone(false);
      _enumerator.Dispose();
    }

    public bool ToggleMicrophone(bool? forceMute = null) {
      _microphone.AudioEndpointVolume.Mute = forceMute.HasValue ? forceMute.Value : !_microphone.AudioEndpointVolume.Mute;
      LogMicrophoneStatus();
      return _microphone.AudioEndpointVolume.Mute;
    }

    private void LogMicrophoneStatus() {
      Console.WriteLine($"Microphone is muted: {_microphone.AudioEndpointVolume.Mute}");
    }
  }
}
