using UnityEngine;
using GraphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat;
using IntPtr = System.IntPtr;

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
        (Texture2D color, Texture2D position) _temporaries;

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
                new Texture2D(2048 * 6, 1536, GraphicsFormat.R8_UNorm, 0)
            );
        }

        void OnDestroy()
        {
            if (_material != null) Destroy(_material);
            if (_temporaries.color != null) Destroy(_temporaries.color);
            if (_temporaries.position != null) Destroy(_temporaries.position);
            _driver?.Dispose();
        }

        unsafe void Update()
        {
            // Try retrieving the last frame.
            var (cmem, pmem) = _driver.LockLastFrame();
            if (cmem.IsEmpty || pmem.IsEmpty) return;

            // Load the frame data into the temporary textures.
            using (var handle = cmem.Pin())
                _temporaries.color.LoadRawTextureData((IntPtr)handle.Pointer, cmem.Length);

            using (var handle = pmem.Pin())
                _temporaries.position.LoadRawTextureData((IntPtr)handle.Pointer, pmem.Length);

            _temporaries.color.Apply();
            _temporaries.position.Apply();

            // We don't need the last frame any more.
            _driver.ReleaseLastFrame();

            // Update the external textures.
            Graphics.Blit(_temporaries.color, _colorTexture);

            _material.SetTexture("_SourceTexture", _temporaries.position);
            _material.SetVector("_Dimensions", new Vector2(2048, 1536));
            Graphics.Blit(null, _positionTexture, _material, 0);
        }

        #endregion
    }
}
