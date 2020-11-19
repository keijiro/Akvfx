using UnityEngine;

namespace Akvfx {

public sealed class DeviceController : MonoBehaviour
{
    #region Editable attribute

    [SerializeField] DeviceSettings _deviceSettings = null;

    public DeviceSettings DeviceSettings
      { get => _deviceSettings; set => SetDeviceSettings(value); }

    #endregion

    #region Asset reference

    [SerializeField, HideInInspector] ComputeShader _compute = null;

    #endregion

    #region Public accessor properties

    public RenderTexture ColorMap => _colorMap;
    public RenderTexture PositionMap => _positionMap;

    #endregion

    #region Private members

    ThreadedDriver _driver;
    ComputeBuffer _xyTable;
    ComputeBuffer _colorBuffer;
    ComputeBuffer _depthBuffer;
    RenderTexture _colorMap;
    RenderTexture _positionMap;

    void SetDeviceSettings(DeviceSettings settings)
    {
        _deviceSettings = settings;
        if (_driver != null) _driver.Settings = settings;
    }

    #endregion

    #region Shader property IDs

    static class ID
    {
        public static int ColorBuffer = Shader.PropertyToID("ColorBuffer");
        public static int DepthBuffer = Shader.PropertyToID("DepthBuffer");
        public static int XYTable     = Shader.PropertyToID("XYTable");
        public static int MaxDepth    = Shader.PropertyToID("MaxDepth");
        public static int ColorMap    = Shader.PropertyToID("ColorMap");
        public static int PositionMap = Shader.PropertyToID("PositionMap");
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Start capturing via the threaded driver.
        _driver = new ThreadedDriver(_deviceSettings);

        // Temporary objects for conversion
        var width = ThreadedDriver.ImageWidth;
        var height = ThreadedDriver.ImageHeight;

        _colorBuffer = new ComputeBuffer(width * height, 4);
        _depthBuffer = new ComputeBuffer(width * height / 2, 4);

        _colorMap = new RenderTexture
          (width, height, 0, RenderTextureFormat.Default);
        _colorMap.enableRandomWrite = true;
        _colorMap.Create();

        _positionMap = new RenderTexture
          (width, height, 0, RenderTextureFormat.ARGBFloat);
        _positionMap.enableRandomWrite = true;
        _positionMap.Create();
    }

    void OnDestroy()
    {
        if (_colorMap    != null) Destroy(_colorMap);
        if (_positionMap != null) Destroy(_positionMap);

        _colorBuffer?.Dispose();
        _depthBuffer?.Dispose();

        _xyTable?.Dispose();
        _driver?.Dispose();
    }

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

        // Invoke the unprojection compute shader.
        _compute.SetFloat(ID.MaxDepth, _deviceSettings.maxDepth);
        _compute.SetBuffer(0, ID.ColorBuffer, _colorBuffer);
        _compute.SetBuffer(0, ID.DepthBuffer, _depthBuffer);
        _compute.SetBuffer(0, ID.XYTable, _xyTable);
        _compute.SetTexture(0, ID.ColorMap, _colorMap);
        _compute.SetTexture(0, ID.PositionMap, _positionMap);
        _compute.Dispatch(0, _colorMap.width / 8, _colorMap.height / 8, 1);
    }

    #endregion
}

}
