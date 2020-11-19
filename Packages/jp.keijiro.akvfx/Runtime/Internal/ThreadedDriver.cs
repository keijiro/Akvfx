using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Akvfx {

sealed class ThreadedDriver : IDisposable
{
    #region Public properties and methods

    public static int ImageWidth => 640;
    public static int ImageHeight => 576;

    public ThreadedDriver(DeviceSettings settings)
    {
        // FIXME: Dangerous. We should do this only on Player.
        K4aExtensions.DisableSafeCopyNativeBuffers();

        Settings = settings;

        _captureThread = new Thread(CaptureThread);
        _captureThread.Start();
    }

    public void Dispose()
    {
        _terminate = true;
        _captureThread.Join();

        TrimQueue(0);
        ReleaseLastFrame();

        GC.SuppressFinalize(this);
    }

    public DeviceSettings Settings { get; set; }

    public ReadOnlySpan<float> XYTable
      => _xyTable != null ? _xyTable.Data : null;

    public (ReadOnlyMemory<byte> color, ReadOnlyMemory<byte> depth)
       LockLastFrame()
    {
        // Try retrieving the last frame.
        if (_lockedFrame.capture == null) _queue.TryDequeue(out _lockedFrame);

        // Return null if it failed to retrieve.
        if (_lockedFrame.capture == null) return (null, null);

        return (_lockedFrame.color.Memory, _lockedFrame.capture.Depth.Memory);
    }

    public void ReleaseLastFrame()
    {
        _lockedFrame.capture?.Dispose();
        _lockedFrame.color?.Dispose();
        _lockedFrame = (null, null);
    }

    #endregion

    #region Private objects

    XYTable _xyTable;

    #endregion

    #region Capture queue

    ConcurrentQueue<(Capture capture, Image color)>
        _queue = new ConcurrentQueue<(Capture, Image)>();

    (Capture capture, Image color) _lockedFrame;

    // Trim the queue to a specified count.
    void TrimQueue(int count)
    {
        while (_queue.Count > count)
        {
            (Capture capture, Image color) temp;
            _queue.TryDequeue(out temp);
            temp.capture?.Dispose();
            temp.color?.Dispose();
        }
    }

    #endregion

    #region Capture thread

    Thread _captureThread;
    bool _terminate;

    void CaptureThread()
    {
        // If there is no available device, do nothing.
        if (Device.GetInstalledCount() == 0) return;

        // Open the default device.
        var device = Device.Open();

        // Start capturing with custom settings.
        device.StartCameras
          (new DeviceConfiguration
            { ColorFormat = ImageFormat.ColorBGRA32,
              ColorResolution = ColorResolution.R1536p, // 2048 x 1536 (4:3)
              DepthMode = DepthMode.NFOV_Unbinned,      // 640x576
              SynchronizedImagesOnly = true });

        // Construct XY table as a background task.
        Task.Run(() => _xyTable =
          new XYTable(device.GetCalibration(), ImageWidth, ImageHeight));

        // Set up the transformation object.
        var transformation = new Transformation(device.GetCalibration());

        // Initially apply the device settings.
        var setter = new DeviceSettingController(device, Settings);

        while (!_terminate)
        {
            // Get a frame capture.
            var capture = device.GetCapture();

            // Transform the color image to the depth perspective.
            var color = transformation.ColorImageToDepthCamera(capture);

            // Push the frame to the capture queue.
            _queue.Enqueue((capture, color));

            // Remove old frames.
            TrimQueue(1);

            // Apply changes on the device settings.
            setter.ApplySettings(device, Settings);
        }

        // Cleaning up.
        transformation.Dispose();
        device.Dispose();
    }

    #endregion
}

}
