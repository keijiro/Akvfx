using UnityEngine;
using System;
using System.Reflection;

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
    }

    static class ComputeBufferExtensions
    {
        // SetData with ReadOnlySpan
        public unsafe static void SetData<T>
          (this ComputeBuffer buffer, ReadOnlySpan<T> data) where T : unmanaged
        {
            fixed (T* pData = &data.GetPinnableReference())
                buffer.SetData((IntPtr)pData, data.Length, sizeof(T));
        }

        // Directly load an unmanaged data array to a compute buffer via an
        // Intptr. This is not a public interface so will be broken one day.
        // DO NOT TRY AT HOME.
        static MethodInfo _method;

        static MethodInfo Method
          => _method ?? (_method = GetMethod());

        static MethodInfo GetMethod()
          => typeof(ComputeBuffer).GetMethod("InternalSetNativeData",
                                             BindingFlags.InvokeMethod |
                                             BindingFlags.NonPublic |
                                             BindingFlags.Instance);

        static object [] _args5 = new object[5];

        public static void SetData
          (this ComputeBuffer buffer, IntPtr pointer, int count, int stride)
        {
            _args5[0] = pointer;
            _args5[1] = 0;      // source offset
            _args5[2] = 0;      // buffer offset
            _args5[3] = count;
            _args5[4] = stride;

            Method.Invoke(buffer, _args5);
        }
    }
}
