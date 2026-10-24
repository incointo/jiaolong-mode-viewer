using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace JiaolongModeViewer
{
    internal static class EcReader
    {
        // This program contains only the OEM EC READ IOCTL. No write IOCTL.
        private const uint EcRead = 2621482120;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string name, uint access, uint share,
            IntPtr security, uint creation, uint flags, IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(SafeFileHandle device, uint code,
            ref uint input, uint inputSize, out uint output, uint outputSize,
            out uint returned, IntPtr overlapped);

        internal static void ValidateIdentity()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS"))
            {
                if (key == null) throw new InvalidOperationException("无法读取本机型号，未访问驱动。");
                string maker = Convert.ToString(key.GetValue("SystemManufacturer"));
                string board = Convert.ToString(key.GetValue("BaseBoardProduct"));
                string bios = Convert.ToString(key.GetValue("BIOSVersion"));
                if (!string.Equals(maker, "MECHREVO", StringComparison.OrdinalIgnoreCase)
                    || board != "GM6BGEQ" || bios != "N.1.05MRO02")
                    throw new InvalidOperationException("未适配此机型或 BIOS：" + maker + " / " + board + " / " + bios);
            }
        }

        internal static ModeSnapshot Read()
        {
            ValidateIdentity();
            // The OEM driver requires this handle access even for READ requests.
            using (SafeFileHandle handle = CreateFile(@"\\.\ACPIDriver", 0xC0000000,
                3, IntPtr.Zero, 3, 0x80, IntPtr.Zero))
            {
                if (handle.IsInvalid)
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "无法打开原厂驱动，请允许管理员运行，并保留原厂 ACPI 驱动。");
                byte mode = ReadByte(handle, 0x751);
                byte custom = ReadByte(handle, 0x726);
                byte modeAgain = ReadByte(handle, 0x751);
                byte customAgain = ReadByte(handle, 0x726);
                if (mode != modeAgain || custom != customAgain)
                    throw new IOException("模式正在变化，本次未确认；请等待下次刷新。");
                return new ModeSnapshot(mode, custom);
            }
        }

        private static byte ReadByte(SafeFileHandle handle, uint address)
        {
            uint output, returned;
            if (!DeviceIoControl(handle, EcRead, ref address, 4, out output, 4, out returned, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "原厂驱动读取失败。");
            Thread.Sleep(10);
            if (returned != 4 || output > 255) throw new IOException("驱动返回的数据无效，本次未确认。");
            return (byte)output;
        }
    }
}
