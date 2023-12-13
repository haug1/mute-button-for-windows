using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MuteButton.Hooks {
  class KeyboardHook : IDisposable {
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYUP = 0x0101;
    private readonly LowLevelKeyboardProc? _proc = null;
    private readonly IntPtr _hookID;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr _setHook(LowLevelKeyboardProc proc) {
      using Process? curProcess = Process.GetCurrentProcess();
      using ProcessModule? curModule = curProcess.MainModule!;
      return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    }

    public delegate void OnKeyUpEvent(Keys key);
    public event OnKeyUpEvent? OnKeyUp;

    public KeyboardHook() {
      _proc = _hookCallback;
      _hookID = _setHook(_proc);
    }

    private IntPtr _hookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
      if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
        Keys key = (Keys)Marshal.ReadInt32(lParam);
        OnKeyUp?.Invoke(key);
      }
      return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    public void Dispose() {
      UnhookWindowsHookEx(_hookID);
    }
  }
}
