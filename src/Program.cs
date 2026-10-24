using System;
using System.Windows.Forms;

namespace JiaolongModeViewer
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length == 1 && args[0] == "--diagnose")
            {
                // WinExe has no console; write UTF-8 to redirected streams without SetConsoleOutputCP.
                Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput(),
                    new System.Text.UTF8Encoding(false)) { AutoFlush = true });
                Console.SetError(new System.IO.StreamWriter(Console.OpenStandardError(),
                    new System.Text.UTF8Encoding(false)) { AutoFlush = true });
                try
                {
                    ModeSnapshot snapshot = EcReader.Read();
                    Console.WriteLine("Mode=" + snapshot.Name + "; EC0751=0x"
                        + snapshot.ModeByte.ToString("X2") + "; EC0726=0x"
                        + snapshot.CustomByte.ToString("X2") + "; Time=" + snapshot.ReadAt.ToString("o"));
                    return 0;
                }
                catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ViewerForm());
            return 0;
        }
    }
}
