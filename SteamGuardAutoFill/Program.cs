using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SteamAuth;

namespace SteamGuardAutoFill
{
    class MainClass
    {
        private static readonly uint CF_UNICODETEXT = 13;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("user32.dll")]
        static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        public static async Task Main(string[] args)
        {
            await Service();
        }

        private static readonly string SharedSecret = ConfigurationManager.AppSettings["SharedSecret"];

        private static readonly string TargetWindowTitle = ConfigurationManager.AppSettings["TargetWindowTitle"];

        private static readonly int CheckInterval = int.Parse(ConfigurationManager.AppSettings["CheckInterval"]);

        [STAThread]
        private static async Task Service()
        {
            while (true)
            {
                try
                {
                    CheckSteam();
                } catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
                await Task.Delay(CheckInterval);
            }
        }

        private static void CheckSteam()
        {
            if (GetForegroundWindowText() == TargetWindowTitle)
            {
                Fill2FACode();
            }
        }

        private static void Fill2FACode()
        {
            var account = new SteamGuardAccount()
            {
                SharedSecret = SharedSecret
            };
            var code = account.GenerateSteamGuardCode();
            Copy(code);
            SendKeys.SendWait("^V{ENTER}");
        }

        private static void Copy(string code)
        {
            string nullTerminatedStr = code + "\0";
            byte[] strBytes = Encoding.Unicode.GetBytes(nullTerminatedStr);
            IntPtr hglobal = Marshal.AllocHGlobal(strBytes.Length);
            Marshal.Copy(strBytes, 0, hglobal, strBytes.Length);
            OpenClipboard(IntPtr.Zero);
            EmptyClipboard();
            SetClipboardData(CF_UNICODETEXT, hglobal);
            CloseClipboard();
            Marshal.FreeHGlobal(hglobal);
        }

        private static string GetForegroundWindowText()
        {
            const int nChars = 256;
            IntPtr handle;
            StringBuilder sb = new StringBuilder(nChars);
            handle = GetForegroundWindow();
            if (GetWindowText(handle, sb, nChars) > 0)
            {
                return sb.ToString();
            } else
            {
                return null;
            }
        }
    }
}
