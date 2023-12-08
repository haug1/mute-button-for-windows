using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace mute_button {
  class Program {

    [STAThread]
    static void Main(string[] args) {
      var engine = new Engine();
      Console.WriteLine("Initialization completed");
      Console.CancelKeyPress += delegate {
        // Call dispose when CTRL+C on command line
        engine.Dispose();
      };
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run();
      engine.Dispose(); // Call dispose when exiting gracefully
    }
  }
}

