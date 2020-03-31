using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;

namespace Akvfx
{
    public sealed class DeviceSettings : ScriptableObject
    {
        #region Editable fields

        [SerializeField] bool _autoExposure = true;
        [SerializeField, Range(-11, 1)] int _exposure = -5;

        [SerializeField] bool _autoWhiteBalance = true;
        [SerializeField, Range(2500, 10000)] float _whiteBalance = 3200;

        [SerializeField, Range(0, 1)] float _brightness = 0.5f;
        [SerializeField, Range(0, 1)] float _contrast = 0.5f;
        [SerializeField, Range(0, 1)] float _saturation = 0.5f;
        [SerializeField, Range(0, 1)] float _sharpness = 0.5f;
        [SerializeField, Range(0, 1)] float _gain = 1;

        [SerializeField] bool _enableBlc = false;
        [SerializeField] bool _powerIs60Hz = true;

        [SerializeField, Range(0, 6.6f)] float _maxDepth = 1;

        [SerializeField] DepthMode _depthMode = DepthMode.NFOV_Unbinned;

        #endregion

        #region Public accessors

        public bool autoExposure {
            get { return _autoExposure; }
            set { _autoExposure = value; }
        }

        public int exposure {
            get { return _exposure; }
            set { _exposure = value; }
        }

        public bool autoWhiteBalance {
            get { return _autoWhiteBalance; }
            set { _autoWhiteBalance = value; }
        }

        public float whiteBalance {
            get { return _whiteBalance; }
            set { _whiteBalance = value; }
        }

        public float brightness {
            get { return _brightness; }
            set { _brightness = value; }
        }

        public float contrast {
            get { return _contrast; }
            set { _contrast = value; }
        }

        public float saturation {
            get { return _saturation; }
            set { _saturation = value; }
        }

        public float sharpness {
            get { return _sharpness; }
            set { _sharpness = value; }
        }

        public float gain {
            get { return _gain; }
            set { _gain = value; }
        }

        public bool enableBlc {
            get { return _enableBlc; }
            set { _enableBlc = value; }
        }

        public bool powerIs60Hz {
            get { return _powerIs60Hz; }
            set { _powerIs60Hz = value; }
        }

        public float maxDepth {
            get { return _maxDepth; }
            set { _maxDepth = value; }
        }

        public DepthMode depthMode {
            get { return _depthMode; }
            set { _depthMode = value; }
        }

        #endregion

        #region Internal properties

        internal int ExposureDeviceValue { get {
            if (_autoExposure) return -1;
            return (int)(Mathf.Pow(2, _exposure) * 1000 * 1000);
        } }

        internal int WhiteBalanceDeviceValue { get {
            if (_autoWhiteBalance) return -1;
            // Should be divisible by 10.
            return (int)(_whiteBalance / 10) * 10;
        } }

        internal int BrightnessDeviceValue { get {
            return (int)Mathf.Lerp(0, 255, _brightness);
        } }

        internal int ContrastDeviceValue { get {
            return (int)Mathf.Lerp(0, 10, _contrast);
        } }

        internal int SaturationDeviceValue { get {
            return (int)Mathf.Lerp(0, 63, _saturation);
        } }

        internal int SharpnessDeviceValue { get {
            return (int)Mathf.Lerp(0, 4, _sharpness);
        } }

        internal int GainDeviceValue { get {
            return (int)Mathf.Lerp(0, 255, _gain);
        } }

        internal int BlcDeviceValue { get {
            return _enableBlc ? 1 : 0;
        } }

        internal int PowerFreqDeviceValue { get {
            return _powerIs60Hz ? 2 : 1;
        } }

        #endregion
    }
}
