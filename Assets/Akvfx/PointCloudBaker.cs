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
        (Texture2D color, Texture2D depth) _temporaries;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            // Start capturing via the threaded driver.
            _driver = new ThreadedDriver(_deviceSettings);

            // Temporary objects for convertion shader
            _material = new Material(_shader);
            _temporaries = (
                new Texture2D(2048, 1536, GraphicsFormat.B8G8R8A8_SRGB, 0),
                new Texture2D(2048 * 2, 1536, GraphicsFormat.R8_UNorm, 0)
            );
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
            _xyTable?.Dispose();
            if (_temporaries.color != null) Destroy(_temporaries.color);
            if (_temporaries.depth != null) Destroy(_temporaries.depth);
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

            // Load the frame data into the temporary textures.
            _temporaries.color.LoadRawTextureData(color.Span);
            _temporaries.depth.LoadRawTextureData(depth.Span);
            _temporaries.color.Apply();
            _temporaries.depth.Apply();

            // We don't need the last frame any more.
            _driver.ReleaseLastFrame();

            // Invoke the unprojection shader.
            _material.SetTexture("_ColorTexture", _temporaries.color);
            _material.SetTexture("_DepthTexture", _temporaries.depth);
            _material.SetBuffer("_XYTable", _xyTable);
            _material.SetFloat("_MaxDepth", _deviceSettings.maxDepth);
            var prevRT = RenderTexture.active;
            GraphicsExtensions.SetRenderTarget(_colorTexture, _positionTexture);
            Graphics.Blit(null, _material, 0);
            RenderTexture.active = prevRT;
        }

        #endregion
    }
}
