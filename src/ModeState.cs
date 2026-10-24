using System;

namespace JiaolongModeViewer
{
    internal static class ModeDecoder
    {
        internal static string Decode(byte mode, byte custom)
        {
            if ((custom & 0x80) != 0) return "自定义模式";
            switch (mode & 0x90)
            {
                case 0x80: return "办公模式";
                case 0x00: return "游戏（均衡）模式";
                case 0x10: return "狂暴模式";
                default: return "未知模式";
            }
        }
    }

    internal sealed class ModeSnapshot
    {
        internal readonly byte ModeByte;
        internal readonly byte CustomByte;
        internal readonly DateTime ReadAt;
        internal string Name { get { return ModeDecoder.Decode(ModeByte, CustomByte); } }

        internal ModeSnapshot(byte mode, byte custom)
        {
            ModeByte = mode;
            CustomByte = custom;
            ReadAt = DateTime.Now;
        }
    }
}
