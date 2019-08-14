//
// This file contains some utility extensions for Unity graphics classes.
// These extensions are mainly for adding Span support to storage classes.
//

using UnityEngine;
using System;
using System.Reflection;

namespace Akvfx
{
    static class GraphicsExtensions
    {
        // MRT with tow render textures
        public static void SetRenderTarget(RenderTexture rt1, RenderTexture rt2)
        {
            _target2[0] = rt1.colorBuffer;
            _target2[1] = rt2.colorBuffer;
            Graphics.SetRenderTarget(_target2, rt1.depthBuffer);
        }

        static RenderBuffer [] _target2 = new RenderBuffer[2];
    }

    static class Texture2DExtensions
    {
        // LoadRawTextureData with ReadOnlySpan
        public unsafe static void LoadRawTextureData
            (this Texture2D texture, ReadOnlySpan<byte> data)
        {
            fixed (byte* pData = &data.GetPinnableReference())
                texture.LoadRawTextureData((IntPtr)pData, data.Length);
        }
    }

    static class ComputeBufferExtensions
    {
        // SetData with ReadOnlySpan
        public unsafe static void SetData<T>
            (this ComputeBuffer buffer, ReadOnlySpan<T> data)
            where T : unmanaged
        {
            fixed (T* pData = &data.GetPinnableReference())
                buffer.SetData((IntPtr)pData, data.Length, sizeof(T));
        }

        // Directly load an unmanaged memory block to a compute buffer via an
        // Intptr. This is not a public interface so will be broken one day.
        // DO NOT TRY AT HOME.
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

        static MethodInfo _method;
        static object [] _args5 = new object[5];
    }
}
