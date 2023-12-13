using NAudio.Wave;

namespace MuteButton.Audio {
  public static class SoundPlayer {
    public static async Task PlaySound(byte[] bytes, CancellationToken cancellationToken) {
      using var memStream = new MemoryStream(bytes);
      using var mp3 = new Mp3FileReader(memStream);
      using var waveOut = new WaveOutEvent();
      waveOut.Init(mp3);
      waveOut.Volume = 0.05f;
      waveOut.Play();
      var tcs = new TaskCompletionSource<object>();
      _ = Task.Run(async () => {
        while (waveOut.PlaybackState == PlaybackState.Playing) {
          await Task.Delay(100);

          if (cancellationToken.IsCancellationRequested) {
            waveOut.Stop();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            tcs.TrySetResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return;
          }
        }
      }, cancellationToken);

      await tcs.Task;
    }
  }
}
