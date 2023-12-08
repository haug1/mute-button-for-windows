using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace mute_button {
  class KeyboardHook : IDisposable {
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYUP = 0x0101;
    private readonly LowLevelKeyboardProc _proc;
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

    private IntPtr SetHook(LowLevelKeyboardProc proc) {
      using (Process curProcess = Process.GetCurrentProcess())
      using (ProcessModule curModule = curProcess.MainModule) {
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
      }
    }

    public delegate void OnKeyUp(Keys key);

    private readonly OnKeyUp _onKeyUp;

    public KeyboardHook(OnKeyUp onKeyUp) {
      _proc = HookCallback;
      _hookID = SetHook(_proc);
      _onKeyUp = onKeyUp;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
      if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) {
        Keys key = (Keys)Marshal.ReadInt32(lParam);
        _onKeyUp(key);
      }
      return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    public void Dispose() {
      UnhookWindowsHookEx(_hookID);
    }
  }
}
