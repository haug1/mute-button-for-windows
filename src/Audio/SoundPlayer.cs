using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;

namespace MuteButton.Audio {
  public static class SoundPlayer {
    public static async Task PlaySound(string resourceName, CancellationToken cancellationToken) {
      try {
        using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        using var waveOut = new WaveOutEvent();
        using var reader = new Mp3FileReader(resourceStream);
        waveOut.Init(reader);
        waveOut.Volume = 0.05f;
        waveOut.Play();

        var tcs = new TaskCompletionSource<object>();

        _ = Task.Run(async () => {
          while (waveOut.PlaybackState == PlaybackState.Playing) {
            await Task.Delay(100);

            if (cancellationToken.IsCancellationRequested) {
              waveOut.Stop();
              tcs.TrySetResult(null);
              return;
            }
          }
        }, cancellationToken);

        await tcs.Task;
      } catch (Exception e) {
        Console.WriteLine(e);
      }
    }
  }
}
