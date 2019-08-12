using Microsoft.Azure.Kinect.Sensor;
using System.Collections.Concurrent;
using System.Threading;

using ByteMemory = System.Memory<byte>;

namespace Akvfx
{
    sealed class ThreadedDriver : System.IDisposable
    {
        #region Public methods

        public ThreadedDriver(DeviceSettings settings)
        {
            // FIXME: Dangerous. We should do this only on Player.
            UnsafeUtility.DisableSafeCopyNativeBuffers();

            _settings = settings;

            _captureThread = new Thread(CaptureThread);
            _captureThread.Start();
        }

        public void Dispose()
        {
            _terminate = true;
            _captureThread.Join();

            TrimQueue(0);
            ReleaseLastFrame();

            System.GC.SuppressFinalize(this);
        }

        public (ByteMemory color, ByteMemory position) LockLastFrame()
        {
            // Try retrieving the last frame.
            if (_lockedFrame.capture == null)
                _queue.TryDequeue(out _lockedFrame);

            // Return null if it failed to retrieve.
            if (_lockedFrame.capture == null) return (null, null);

            return (
                _lockedFrame.capture.Color.Memory,
                _lockedFrame.position.Memory
            );
        }

        public void ReleaseLastFrame()
        {
            _lockedFrame.capture?.Dispose();
            _lockedFrame.position?.Dispose();
            _lockedFrame = (null, null);
        }

        #endregion

        #region Private objects

        DeviceSettings _settings;

        #endregion

        #region Capture queue

        ConcurrentQueue<(Capture capture, Image position)>
            _queue = new ConcurrentQueue<(Capture, Image)>();

        (Capture capture, Image position) _lockedFrame;

        // Trim the queue to a specified count.
        void TrimQueue(int count)
        {
            while (_queue.Count > count)
            {
                (Capture capture, Image position) temp;
                _queue.TryDequeue(out temp);
                temp.capture?.Dispose();
                temp.position?.Dispose();
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
            device.StartCameras(
                new DeviceConfiguration {
                    ColorFormat = ImageFormat.ColorBGRA32,
                    ColorResolution = ColorResolution.R1536p, // 2048 x 1536 (4:3)
                    DepthMode = DepthMode.NFOV_Unbinned,      // 640x576
                    SynchronizedImagesOnly = true
                }
            );

            // Set up the transformation object.
            var transformation = new Transformation(device.GetCalibration());

            // Initially apply the device settings.
            var setter = new DeviceSettingController(device, _settings);

            while (!_terminate)
            {
                // Get a frame capture.
                var capture = device.GetCapture();

                // Transform the depth image to the color perspective.
                var depth = transformation.DepthImageToColorCamera(capture);

                // Unproject the depth samples and reconstruct a point cloud.
                var pointCloud = transformation.DepthImageToPointCloud
                    (depth, CalibrationDeviceType.Color);

                // Transformed depth is not needed any more.
                depth.Dispose();

                // Push the frame to the capture queue.
                _queue.Enqueue((capture, pointCloud));

                // Remove old frames.
                TrimQueue(1);

                // Apply changes on the device settings.
                setter.ApplySettings(device, _settings);
            }

            // Cleaning up.
            transformation.Dispose();
            device.Dispose();
        }

        #endregion
    }
}
