// author：      Administrator
// created time：2014/4/17 17:38:46
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18444
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace CURELab.SignLanguage.HandDetector
{


    /// <summary>
    /// add summary here
    /// </summary>
    /// 
    public static class win32API
    {

        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_SETTEXT = 0x000C;
        public const int BM_CLICK = 0x00F5;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int VK_RETURN = 0x0D;
        public const int VK_MENU = 0x12;
        public const int VK_RIGHT = 0x27;
        
        public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("User32.Dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, int lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImportAttribute("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory
        (
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        IntPtr lpBuffer,
        int nSize,
        IntPtr lpNumberOfBytesRead
        );
        [DllImportAttribute("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess
        (
        int dwDesiredAccess,
        bool bInheritHandle,
        int dwProcessId
        );
        [DllImport("kernel32.dll")]
        public static extern void CloseHandle
        (
        IntPtr hObject
        );
        //写内存
        [DllImportAttribute("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory
        (
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        int[] lpBuffer,
        int nSize,
        IntPtr lpNumberOfBytesWritten
        );
        
    }
}