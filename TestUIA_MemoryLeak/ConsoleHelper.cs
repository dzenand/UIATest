using System.Runtime.InteropServices;

namespace TestUIA
{
    public class ConsoleHelper
    {
        private const string Kernel32Dll = "kernel32.dll";

        [DllImport(Kernel32Dll, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int AllocConsole();

        [DllImport(Kernel32Dll, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool FreeConsole();


        public static void OpenConsole()
        {
            AllocConsole();
        }

        public static void CloseConsole()
        {
            FreeConsole();
        }         
    }
}