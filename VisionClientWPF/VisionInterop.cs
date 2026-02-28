using System.Runtime.InteropServices;

namespace VisionClientWPF
{
    public static class VisionInterop
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DetectionResult
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public bool detected;
        }

        [DllImport("NeuroC_ComVision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartCamera();

        [DllImport("NeuroC_ComVision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetFrame(out DetectionResult result);

        [DllImport("NeuroC_ComVision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopCamera();
    }
}