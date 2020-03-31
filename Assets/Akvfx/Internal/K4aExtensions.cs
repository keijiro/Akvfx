using System.Reflection;
using Microsoft.Azure.Kinect.Sensor;

namespace Akvfx
{
    static class K4aExtensions
    {
        // Set false to Allocator.SafeCopyNativeBuffers. You can earn a few
        // milliseconds by skipping safe-copy. Note that it turns minor bugs
        // undebuggable crashes.
        public static void DisableSafeCopyNativeBuffers()
        {
            var allocator = System.Type.GetType(
                "Microsoft.Azure.Kinect.Sensor.Allocator,Microsoft.Azure.Kinect.Sensor"
            );

            var singleton = allocator.GetProperty(
                "Singleton",
                BindingFlags.Public | BindingFlags.Static
            );

            var safeCopyNativeBuffers = allocator.GetProperty(
                "SafeCopyNativeBuffers",
                BindingFlags.Public | BindingFlags.Instance
            );

            safeCopyNativeBuffers.SetValue(singleton.GetValue(null), false);
        }

        /// <summary> Return the width of the associated DepthMode Texture </summary>
        public static int Width(this DepthMode mode) {
            switch (mode) {
                case DepthMode.Off:
                    return 0;
                case DepthMode.NFOV_2x2Binned:
                    return 320;
                case DepthMode.NFOV_Unbinned:
                    return 640;
                case DepthMode.PassiveIR:
                    return 1024;
                case DepthMode.WFOV_2x2Binned:
                    return 512;
                case DepthMode.WFOV_Unbinned:
                    return 1024;
            }
            return 0;
        }

        /// <summary> Return the height of the associated DepthMode Texture </summary>
        public static int Height(this DepthMode mode) {
            switch (mode) {
                case DepthMode.Off:
                    return 0;
                case DepthMode.NFOV_2x2Binned:
                    return 288;
                case DepthMode.NFOV_Unbinned:
                    return 576;
                case DepthMode.PassiveIR:
                    return 1024;
                case DepthMode.WFOV_2x2Binned:
                    return 512;
                case DepthMode.WFOV_Unbinned:
                    return 1024;
            }
            return 0;
        }
    }
}
