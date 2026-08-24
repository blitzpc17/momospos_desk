using System;
using System.Runtime.InteropServices;

namespace momospos.Helpers
{
    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "Ticket POS";
            di.pDataType = "RAW";

            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
            bool bSuccess = false;
            try
            {
                Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                bSuccess = SendBytesToPrinter(printerName, pUnmanagedBytes, bytes.Length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }
            return bSuccess;
        }

        public static void OpenCashDrawer(string printerName)
        {
            try
            {
                // ESC p m t1 t2
                // m = 0, t1 = 25, t2 = 250
                byte[] cashDrawerOpenCommand = new byte[] { 27, 112, 0, 25, 250 };
                IntPtr pUnmanagedBytes = new IntPtr(0);
                int nLength = cashDrawerOpenCommand.Length;
                pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                Marshal.Copy(cashDrawerOpenCommand, 0, pUnmanagedBytes, nLength);
                SendBytesToPrinter(printerName, pUnmanagedBytes, nLength);
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }
            catch (Exception)
            {
                // Ignore any error opening the cash drawer
            }
        }

        public static void CutPaper(string printerName)
        {
            try
            {
                // ESC/POS multiple cut commands to ensure compatibility
                // GS V 1 (0x1D, 0x56, 0x01) - Standard Epson
                // ESC i (0x1B, 0x69) - Full cut (some printers)
                // ESC m (0x1B, 0x6D) - Partial cut
                byte[] cutPaperCommand = new byte[] { 
                    29, 86, 1, 
                    27, 105, 
                    27, 109 
                };
                IntPtr pUnmanagedBytes = new IntPtr(0);
                int nLength = cutPaperCommand.Length;
                pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);
                Marshal.Copy(cutPaperCommand, 0, pUnmanagedBytes, nLength);
                SendBytesToPrinter(printerName, pUnmanagedBytes, nLength);
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }
            catch (Exception)
            {
                // Ignore any error
            }
        }
    }
}
