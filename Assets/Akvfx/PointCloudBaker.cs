using UnityEngine;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;

namespace Akvfx
{
    public sealed class PointCloudBaker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] DeviceSettings _deviceSettings = null;
        [SerializeField] RenderTexture _colorTexture = null;
        [SerializeField] RenderTexture _positionTexture = null;
        [SerializeField, HideInInspector] Shader _shader = null;

        #endregion

        #region Internal objects

        ThreadedDriver _driver;
        Material _material;
        ComputeBuffer _xyTable;
        ComputeBuffer _colorBuffer;
        ComputeBuffer _depthBuffer;

        #endregion

        #region Shader property IDs

        static class ID
        {
            public static readonly int ColorBuffer = Shader.PropertyToID("_ColorBuffer");
            public static readonly int DepthBuffer = Shader.PropertyToID("_DepthBuffer");
            public static readonly int XYTable     = Shader.PropertyToID("_XYTable");
            public static readonly int MaxDepth    = Shader.PropertyToID("_MaxDepth");
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // Start capturing via the threaded driver.
            _driver = new ThreadedDriver(_deviceSettings);

            // Temporary objects for convertion shader
            _material = new Material(_shader);
            _colorBuffer = new ComputeBuffer(2048 * 1536, 4);
            _depthBuffer = new ComputeBuffer(2048 * 1536 / 2, 4);
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
            _xyTable?.Dispose();
            _colorBuffer?.Dispose();
            _depthBuffer?.Dispose();
            _driver?.Dispose();
        }

        RenderBuffer[] _mrt = new RenderBuffer[2];

        unsafe void Update()
        {
            // Try initializing XY table if it's not ready.
            if (_xyTable == null)
            {
                var data = _driver.XYTable;
                if (data.IsEmpty) return; // Table is not ready.

                // Allocate and initialize the XY table.
                _xyTable = new ComputeBuffer(data.Length, sizeof(float));
                _xyTable.SetData(data);
            }

            // Try retrieving the last frame.
            var (color, depth) = _driver.LockLastFrame();
            if (color.IsEmpty || depth.IsEmpty) return;

            // Load the frame data into the compute buffers.
            _colorBuffer.SetData(color.Span);
            _depthBuffer.SetData(depth.Span);

            // We don't need the last frame any more.
            _driver.ReleaseLastFrame();

            // Invoke the unprojection shader.
            _material.SetBuffer(ID.ColorBuffer, _colorBuffer);
            _material.SetBuffer(ID.DepthBuffer, _depthBuffer);
            _material.SetBuffer(ID.XYTable, _xyTable);
            _material.SetFloat(ID.MaxDepth, _deviceSettings.maxDepth);

            var prevRT = RenderTexture.active;
            GraphicsExtensions.SetRenderTarget(_colorTexture, _positionTexture);
            Graphics.Blit(null, _material, 0);
            RenderTexture.active = prevRT;
        }

        #endregion
    }
}
