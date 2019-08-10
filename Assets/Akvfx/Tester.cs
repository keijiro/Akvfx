using UnityEngine;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
using IntPtr = System.IntPtr;
using TimeSpan = System.TimeSpan;
using Microsoft.Azure.Kinect.Sensor;

namespace K4aTest
{
    sealed class Tester : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] RenderTexture _colorTexture = null;
        [SerializeField] RenderTexture _positionTexture = null;
        [SerializeField, HideInInspector] Shader _shader = null;

        #endregion

        #region K4a objects

        Device _device;
        Transformation _transformation;

        #endregion

        #region UnityEngine objects

        Material _material;
        Texture2D _colorTemp;
        Texture2D _pointCloud;

        #endregion

        #region Private functions

        unsafe void UpdateExternalTextures(Image color, Image pointCloud)
        {
            var cmem = color.Memory;

            using (var handle = cmem.Pin())
                _colorTemp.LoadRawTextureData((IntPtr)handle.Pointer, cmem.Length);

            var pmem = pointCloud.Memory;

            using (var handle = pmem.Pin())
                _pointCloud.LoadRawTextureData((IntPtr)handle.Pointer, pmem.Length);

            _colorTemp.Apply();
            _pointCloud.Apply();

            _material.SetTexture("_MainTex", _pointCloud);
            _material.SetVector("_Dimensions", new Vector2(color.WidthPixels, color.HeightPixels));

            Graphics.Blit(_colorTemp, _colorTexture);
            Graphics.Blit(_pointCloud, _positionTexture, _material, 0);
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // If there is no available device, do nothing.
            if (Device.GetInstalledCount() == 0) return;

            // Open the default device.
            _device = Device.Open();
            if (_device == null) return;

            // Start capturing with our settings.
            _device.StartCameras(
                new DeviceConfiguration {
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R1536p, // 2048 x 1536 (4:3)
                    DepthMode = DepthMode.NFOV_Unbinned,      // 640x576
                    SynchronizedImagesOnly = true
                }
            );

            // Prepare the transformation object.
            _transformation = new Transformation(_device.GetCalibration());

            // Temporary objects for convertion shader
            _material = new Material(_shader);
            _colorTemp = new Texture2D(2048, 1536, GraphicsFormat.B8G8R8A8_SRGB, 0);
            _pointCloud = new Texture2D(2048 * 6, 1536, GraphicsFormat.R8_UNorm, 0);
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
            if (_colorTemp != null) Destroy(_colorTemp);
            if (_pointCloud != null) Destroy(_pointCloud);

            _transformation?.Dispose();
            _device?.StopCameras();
            _device?.Dispose();
        }

        void Update()
        {
            // Check if the device is ready.
            if (_device == null || _transformation == null) return;

            try
            {
                // Capture a frame with 1/15 sec timeout.
                // TODO: Threading
                using (var capture = _device.GetCapture(TimeSpan.FromSeconds(1.0 / 15)))
                {
                    // Transform the depth image to the color perspective.
                    using (var depth = _transformation.DepthImageToColorCamera(capture))
                    {
                        // Unproject the depth samples and reconstruct a point cloud.
                        using (var pointCloud = _transformation.DepthImageToPointCloud(depth, CalibrationDeviceType.Color))
                        {
                            UpdateExternalTextures(capture.Color, pointCloud);
                        }
                    }
                }
            }
            catch (System.TimeoutException)
            {
                // Ignore timeouts
            }
        }

        #endregion
    }
}
