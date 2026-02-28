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


        [DllImport("NeuroCComVision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartCamera();

        [DllImport("NeuroCComVision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetFrame(out DetectionResult result);

        [DllImport("NeuroCComVision.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopCamera();
    }
}