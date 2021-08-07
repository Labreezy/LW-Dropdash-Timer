using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DropdashTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out, MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesRead);
        private TimeSpan ts;
        private DispatcherTimer timer;
        private Process process;
        public MainWindow()
        {
            InitializeComponent();
            ts = new TimeSpan(0);
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 16);
            timer.Tick += TimerOnTick;
            process = Process.GetProcessesByName("slw").FirstOrDefault();
            if (process == null)
            {
                MessageBox.Show("Open lost world first");
            }
            else
            {
                timer.Start();
            }
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            int bytesRead = 0;
            IntPtr currentptr = IntPtr.Zero;
            byte[] ptrbuf = new byte[4];
            byte[] inputbuf = new byte[0x18];
            int[] offsets = { 0xC, 0x28 };
            IntPtr processHandle = process.Handle;
            IntPtr processBase = process.MainModule.BaseAddress;
            
                if (ReadProcessMemory(processHandle, IntPtr.Add(processBase, 0xBD3F90), ptrbuf, 4, ref bytesRead) ==
                    false || bytesRead != 4)
                {

                    return;
                }

                currentptr = new IntPtr(BitConverter.ToInt32(ptrbuf, 0));
                foreach (int offset in offsets)
                {
                    currentptr = IntPtr.Add(currentptr, offset);
                    if (ReadProcessMemory(processHandle, currentptr, ptrbuf, 4, ref bytesRead) == false ||
                        bytesRead != 4)
                    {

                        
                        return;
                    }

                    currentptr = new IntPtr(BitConverter.ToInt32(ptrbuf, 0));
                }

                if (ReadProcessMemory(processHandle, currentptr, inputbuf, 0x14, ref bytesRead) == false ||
                    bytesRead != 0x14)
                {
                    
                    return;
                }

                int buttons = BitConverter.ToInt32(inputbuf, 0);
                if ((buttons & 0x400) != 0)
                {
                    ts = ts.Add(timer.Interval);
                    TimeBlock.Text = ts.ToString(@"h\:mm\:ss\.ff");
                }
               
            


        }

        
        private void MainWindow_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            timer.Stop();
            ts = new TimeSpan(0);
            timer.Start();
        }
    }
}