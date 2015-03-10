// author：      Administrator
// created time：2014/5/9 5:56:02
// organizatioin:CURE lab, CUHK
// copyright：   2014-2015
// CLR：         4.0.30319.18444
// project link：https://github.com/huangfuyang/Sign-Language-with-Kinect

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;


namespace CURELab.SignLanguage.HandDetector
{
    /// <summary>
    /// add summary here
    /// </summary>
    public class KinectStudioController
    {

        private static KinectStudioController singleton;
        private IntPtr hWnd = IntPtr.Zero;
        private IntPtr hWindow = IntPtr.Zero;
        private static int p_processid = 0;

        private IntPtr dllEntry = IntPtr.Zero;

        private int FirstFrame { get; set; }
        IntPtr hToolStrip = IntPtr.Zero;

        public static KinectStudioController GetSingleton()
        {
            if (singleton == null)
            {
                singleton = new KinectStudioController();
            }
            return singleton;

        }

        private KinectStudioController()
        {

        }

        public bool Start()
        {
            String path = "C:\\Program Files\\Microsoft SDKs\\Kinect\\Developer Toolkit v1.8.0\\Tools\\KinectStudio\\KinectStudio.exe";
            Process.Start(path);
            Thread.Sleep(1000);
            return Connect();
        }

        public bool Connect()
        {
            FindKinectStudioWindowHandler();
            if (hWnd == IntPtr.Zero)
            {
                return false;
            }

            return true;
        }
        public void Open_File(string path)
        {
            FirstFrame = 0;
            hToolStrip = win32API.FindWindowEx(hWnd, IntPtr.Zero, null, "toolStrip1");
            if (hToolStrip == IntPtr.Zero)
            {
                return;
            }
            int p_processid;
            win32API.GetWindowThreadProcessId(hWnd, out p_processid);
            //click open
            int x = 28, y = 10;
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONDOWN, 0, (y << 16) + x);
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONUP, 0, (y << 16) + x);
            Thread.Sleep(1500);
            win32API.EnumWindows(new win32API.EnumWindowsProc(EnumWindowsFunc), p_processid);
            IntPtr hOpen = hWindow;
            // find folder
            IntPtr hTemp = GetWindowChildBFSByIndex(hOpen, 2);
            IntPtr hEdit = GetWindowChildDFSByDepth(hTemp, 2);
            win32API.SendMessage(hEdit, win32API.WM_SETTEXT, IntPtr.Zero, path);
            // button ok
            IntPtr hOpenButton = GetWindowChildBFSByIndex(hOpen, 5);
            win32API.SendMessage(hOpenButton, win32API.BM_CLICK, 0, 0);

        }
        public void Run()
        {
            hToolStrip = win32API.FindWindowEx(hWnd, IntPtr.Zero, null, "toolStrip1");
            if (hToolStrip == IntPtr.Zero)
            {
                return;
            }
            int x = 260, y = 15;
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONDOWN, 0, (y << 16) + x);
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONUP, 0, (y << 16) + x);
        }

        public bool Run_by_clik()
        {
            hToolStrip = win32API.FindWindowEx(hWnd, IntPtr.Zero, null, "toolStrip1");
            if (hToolStrip == IntPtr.Zero)
            {
                return false;
            }
            int x = 340, y = 15;
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONDOWN, 0, (y << 16) + x);
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONUP, 0, (y << 16) + x);
            return true;
        }

        public void ReadFirstFrame()
        {
            IntPtr dllEntry = IntPtr.Zero;
            Process[] pros = Process.GetProcessesByName("KinectStudio");
            Process pro = pros[0];
            if (pro.ProcessName == "KinectStudio")
            {
                for (int i = 0; i < pro.Modules.Count; i++)
                {
                    if (pro.Modules[i].ModuleName == "KinectStudioNative.dll")
                    {
                        dllEntry = pro.Modules[i].EntryPointAddress;
                        Console.WriteLine("KinectNative.dll addr: {0}", dllEntry.ToString("x8"));
                        dllEntry += Convert.ToInt32("208fbf", 16);
                        Console.WriteLine("Frame addr:" + dllEntry.ToString("x8"));

                    }

                }
            }

            IntPtr tool = win32API.FindWindowEx(hWnd, IntPtr.Zero, null, "toolStrip1");
            int x = 340, y = 10;
            int min = int.MaxValue;
            for (int i = 0; i < 4; i++)
            {

                int tempValue = ReadMemoryValue(dllEntry, pro.Id);
                Console.WriteLine("candidate Frame value:" + tempValue);
                if (tempValue != 0 && tempValue < min)
                {
                    min = tempValue;
                }
                win32API.SendMessage(tool, win32API.WM_LBUTTONDOWN, 0, (y << 16) + x);
                win32API.SendMessage(tool, win32API.WM_LBUTTONUP, 0, (y << 16) + x);
                Thread.Sleep(200);
            }
            FirstFrame = min;

        }

        public void connect_kinect()
        {
            hToolStrip = win32API.FindWindowEx(hWnd, IntPtr.Zero, null, "toolStrip1");
            if (hToolStrip == IntPtr.Zero)
            {
                return;
            }
            int x = 80, y = 15;
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONDOWN, 0, (y << 16) + x);
            win32API.PostMessage(hToolStrip, win32API.WM_LBUTTONUP, 0, (y << 16) + x);
        }

        private bool EnumWindowsFindKinectStudio(IntPtr hWnd, int lParam)
        {

            if (hWnd == IntPtr.Zero)
                return true;		// Not a window
            int buffer = win32API.GetWindowTextLength(hWnd);
            if (buffer == 0)
            {
                return true;
            }
            StringBuilder title = new StringBuilder(buffer + 1);
            win32API.GetWindowText(hWnd, title, title.Capacity);
            if (title == null)
            {
                return true;
            }
            if (title.ToString().Contains("Kinect Studio"))
            {
                this.hWnd = hWnd;
                return false;
            }
            return true;
        }


        private bool EnumWindowsFunc(IntPtr hWnd, int lParam)
        {
            int ProcessId;
            if (hWnd == IntPtr.Zero)
                return true;		// Not a window
            if (!win32API.IsWindowVisible(hWnd))
                return true;
            win32API.GetWindowThreadProcessId(hWnd, out ProcessId);
            if (lParam == ProcessId)
            {
                hWindow = hWnd;
                return false;
            }
            return true;
        }

        private void FindKinectStudioWindowHandler()
        {
            win32API.EnumWindows(new win32API.EnumWindowsProc(EnumWindowsFindKinectStudio), 0);
            if (hWnd != IntPtr.Zero)
            {
                win32API.GetWindowThreadProcessId(hWnd, out p_processid);
            }
        }

        private IntPtr GetWindowChildDFSByDepth(IntPtr window, int index)
        {
            IntPtr child = window;
            for (int i = 0; i < index; i++)
            {
                child = win32API.FindWindowEx(child, IntPtr.Zero, null, null);
                if (child == IntPtr.Zero)
                {
                    return child;
                }
            }
            return child;

        }
        private IntPtr GetWindowChildBFSByIndex(IntPtr window, int index)
        {
            IntPtr child = win32API.FindWindowEx(window, IntPtr.Zero, null, null);
            for (int i = 0; i < index; i++)
            {
                child = win32API.FindWindowEx(window, child, null, null);
                if (child == IntPtr.Zero)
                {
                    return child;
                }
            }
            return child;

        }

        public static int ReadMemoryValue(IntPtr baseAddress, int pid)
        {
            try
            {
                byte[] buffer = new byte[4];
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0); //获取缓冲区地址
                IntPtr hProcess = win32API.OpenProcess(0x1F0FFF, false, pid);
                win32API.ReadProcessMemory(hProcess, (IntPtr)baseAddress, byteAddress, 4, IntPtr.Zero); //将制定内存中的值读入缓冲区
                win32API.CloseHandle(hProcess);
                return Marshal.ReadInt32(byteAddress);
            }
            catch
            {
                return 0;
            }
        }


        public int ReadCurrentFrame()
        {
            Process[] pros = Process.GetProcessesByName("KinectStudio");
            Process pro = pros[0];
            if (dllEntry == IntPtr.Zero)
            {

                if (pro.ProcessName == "KinectStudio")
                {
                    for (int i = 0; i < pro.Modules.Count; i++)
                    {
                        if (pro.Modules[i].ModuleName == "KinectStudioNative.dll")
                        {
                            dllEntry = pro.Modules[i].EntryPointAddress;
                            Console.WriteLine("KinectNative.dll addr: {0}", dllEntry.ToString("x8"));
                            dllEntry += Convert.ToInt32("208fbf", 16);
                            Console.WriteLine("Frame addr:" + dllEntry.ToString("x8"));

                        }

                    }
                }
            }

            int frame = ReadMemoryValue(dllEntry, pro.Id);
            return frame;
        }

    }
}