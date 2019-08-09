using UnityEngine;
using UI = UnityEngine.UI;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
using IntPtr = System.IntPtr;
using TimeSpan = System.TimeSpan;
using Microsoft.Azure.Kinect.Sensor;

namespace K4aTest
{
    sealed class Tester : MonoBehaviour
    {
        [SerializeField] UI.RawImage _colorUI = null;
        [SerializeField] UI.RawImage _depthUI = null;

        Device _device;

        Texture2D _colorTexture;
        Texture2D _depthTexture;

        void Start()
        {
            if (Device.GetInstalledCount() == 0) return;

            _device = Device.Open();
            if (_device == null) return;

            _device.StartCameras(
                new DeviceConfiguration {
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R1536p, // 2048 x 1536 (4:3)
                    DepthMode = DepthMode.NFOV_Unbinned,      // 640x576
                    SynchronizedImagesOnly = true
                }
            );

            _colorTexture = new Texture2D(2048, 1536, GraphicsFormat.B8G8R8A8_SRGB, 0, 0);
            _depthTexture = new Texture2D(640, 576, GraphicsFormat.R16_UNorm, 0, 0);

            _colorUI.texture = _colorTexture;
            _depthUI.texture = _depthTexture;
        }

        void OnDestroy()
        {
            if (_colorTexture != null) Destroy(_colorTexture);
            if (_depthTexture != null) Destroy(_depthTexture);

            _device?.StopCameras();
            _device?.Dispose();
        }

        unsafe void Update()
        {
            try
            {
                using (var capture = _device.GetCapture(TimeSpan.FromSeconds(1.0 / 15)))
                {
                    var color = capture.Color.Memory;
                    using (var handle = color.Pin())
                        _colorTexture.LoadRawTextureData((IntPtr)handle.Pointer, color.Length);

                    var depth = capture.Depth.Memory;
                    using (var handle = depth.Pin())
                        _depthTexture.LoadRawTextureData((IntPtr)handle.Pointer, depth.Length);
                }
            }
            catch (System.TimeoutException)
            {
                // Ignore timeouts
            }

            _colorTexture.Apply();
            _depthTexture.Apply();
        }
    }
}
