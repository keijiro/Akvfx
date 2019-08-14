using System.Reflection;
using ComputeBuffer = UnityEngine.ComputeBuffer;
using IntPtr = System.IntPtr;

namespace Akvfx
{
    static class ComputeBufferExtensions
    {
        static MethodInfo _method;
        static object [] _args5 = new object[5];

        //
        // Directly load an unmanaged data array to a compute buffer via an
        // Intptr. This is not a public interface so will be broken one day.
        // DO NOT TRY AT HOME.
        //
        public static void SetData
            (this ComputeBuffer buffer, IntPtr pointer, int count, int stride)
        {
            if (_method == null)
            {
                _method = typeof(ComputeBuffer).GetMethod(
                    "InternalSetNativeData",
                    BindingFlags.InvokeMethod |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance
                );
            }

            _args5[0] = pointer;
            _args5[1] = 0;      // source offset
            _args5[2] = 0;      // buffer offset
            _args5[3] = count;
            _args5[4] = stride;

            _method.Invoke(buffer, _args5);
        }

        public unsafe static void SetData<T>
            (this ComputeBuffer buffer, System.ReadOnlySpan<T> data)
            where T : unmanaged
        {
            fixed (T* pData = &data.GetPinnableReference())
                buffer.SetData((IntPtr)pData, data.Length, sizeof(T));
        }
    }
}
