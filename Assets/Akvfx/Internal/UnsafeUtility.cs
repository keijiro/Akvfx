using System.Reflection;

namespace Akvfx
{
    static class UnsafeUtility
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
    }
}
