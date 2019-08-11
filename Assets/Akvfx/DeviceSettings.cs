using UnityEngine;

namespace Akvfx
{
    public sealed class DeviceSettings : ScriptableObject
    {
        #region Public properties

        [SerializeField] bool _autoExposure = true;
        [SerializeField, Range(0, 1)] float _exposure = 0.5f;

        [SerializeField] bool _autoWhiteBalance = true;
        [SerializeField, Range(0, 1)] float _whiteBalance = 0.5f;

        [SerializeField, Range(0, 1)] float _brightness = 0.5f;
        [SerializeField, Range(0, 1)] float _contrast = 0.5f;
        [SerializeField, Range(0, 1)] float _saturation = 0.5f;
        [SerializeField, Range(0, 1)] float _sharpness = 0.5f;
        [SerializeField, Range(0, 1)] float _gain = 1;

        [SerializeField] bool _enableBlc = false;
        [SerializeField] bool _powerIs60Hz = true;

        #endregion

        #region Internal methods

        internal int ExposureDeviceValue { get {
            if (_autoExposure) return -1;
            return (int)Mathf.Lerp(488.0f, 1000000.0f, Mathf.Pow(_exposure, 8));
        } }

        internal int WhiteBalanceDeviceValue { get {
            if (_autoWhiteBalance) return -1;
            var x = (int)Mathf.Lerp(2500, 10000, _whiteBalance);
            return x - x % 10; // Should be divisible by 10.
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
