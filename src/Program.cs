using System;
using System.Windows.Forms;
using MuteButton.UI;

namespace MuteButton {
  class Program {
    [STAThread]
    static void Main(string[] args) {
      try {
        Engine engine = new();
        Console.CancelKeyPress += delegate {
          // Call dispose when CTRL+C on command line
          engine.Dispose();
        };
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var mainContext = new MainApplicationContext(engine);
        Console.WriteLine("Initialization completed.");
        Application.Run(mainContext);
        engine.Dispose(); // Call dispose when exiting gracefully
      } catch (Exception ex) {
        Console.WriteLine("ERROR: Application exit");
        Console.WriteLine(ex);
      }
    }
  }
}

