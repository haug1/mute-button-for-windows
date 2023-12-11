using System;
using System.Windows.Forms;

namespace MuteButton {
  class Program {
    [STAThread]
    static void Main(string[] args) {
      try {
        Console.CancelKeyPress += delegate {
          // Call dispose when CTRL+C on command line
          Engine.Instance.Dispose();
        };
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var mainContext = new MainApplicationContext(Engine.Instance);
        Console.WriteLine("Initialization completed.");
        Application.Run(mainContext);
        Engine.Instance.Dispose(); // Call dispose when exiting gracefully
      } catch (Exception ex) {
        Console.WriteLine($"ERROR: Application exit ({ex.Message}): \n{ex.StackTrace}");
      }
    }
  }
}

