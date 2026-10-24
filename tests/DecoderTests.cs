using System;
using JiaolongModeViewer;

internal static class DecoderTests
{
    private static int Main()
    {
        try
        {
            // Real samples collected on this laptop, including unrelated bits.
            Equal("办公模式", ModeDecoder.Decode(0xA0, 0));
            Equal("游戏（均衡）模式", ModeDecoder.Decode(0x20, 0));
            Equal("狂暴模式", ModeDecoder.Decode(0x30, 0));
            Equal("狂暴模式", ModeDecoder.Decode(0x50, 0));
            Equal("未知模式", ModeDecoder.Decode(0x90, 0));
            Equal("自定义模式", ModeDecoder.Decode(0x10, 0x80));
            Equal("自定义模式", ModeDecoder.Decode(0x90, 0x80));
            Console.WriteLine("PASS: 7 real-sample / unknown / custom mode checks");
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e.Message); return 1; }
    }

    private static void Equal(string expected, string actual)
    {
        if (expected != actual) throw new Exception("Expected " + expected + "; got " + actual);
    }
}
