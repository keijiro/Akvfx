using UnityEngine;

namespace Akvfx {

public sealed class DeviceSettings : ScriptableObject
{
    #region Serialized fields

    [SerializeField] bool _autoExposure = true;
    [SerializeField, Range(-11, -6)] int _exposure = -6;

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

    #endregion

    #region Public accessors

    public bool autoExposure
      { get => _autoExposure;
        set => _autoExposure = value; }

    public int exposure
      { get => _exposure;
        set => _exposure = value; }

    public bool autoWhiteBalance
      { get => _autoWhiteBalance;
        set => _autoWhiteBalance = value; }

    public float whiteBalance
      { get => _whiteBalance;
        set => _whiteBalance = value; }

    public float brightness
      { get => _brightness;
        set => _brightness = value; }

    public float contrast
      { get => _contrast;
        set => _contrast = value; }

    public float saturation
      { get => _saturation;
        set => _saturation = value; }

    public float sharpness
      { get => _sharpness;
        set => _sharpness = value; }

    public float gain
      { get => _gain;
        set => _gain = value; }

    public bool enableBlc
      { get => _enableBlc;
        set => _enableBlc = value; }

    public bool powerIs60Hz
      { get => _powerIs60Hz;
        set => _powerIs60Hz = value; }

    public float maxDepth
      { get => _maxDepth;
        set => _maxDepth = value; }

    #endregion

    #region Internal properties

    internal int ExposureDeviceValue
      => _autoExposure ? -1 :
         (_powerIs60Hz ? _exposureTimeTable60 : _exposureTimeTable50)
           [_exposure + 11];

    internal int WhiteBalanceDeviceValue
      => _autoWhiteBalance ? -1 :
         (int)(_whiteBalance / 10) * 10; // Should be divisible by 10.

    internal int BrightnessDeviceValue
      => (int)Mathf.Lerp(0, 255, _brightness);

    internal int ContrastDeviceValue
      => (int)Mathf.Lerp(0, 10, _contrast);

    internal int SaturationDeviceValue
      => (int)Mathf.Lerp(0, 63, _saturation);

    internal int SharpnessDeviceValue
      => (int)Mathf.Lerp(0, 4, _sharpness);

    internal int GainDeviceValue
      => (int)Mathf.Lerp(0, 255, _gain);

    internal int BlcDeviceValue
      => _enableBlc ? 1 : 0;

    internal int PowerFreqDeviceValue
      => _powerIs60Hz ? 2 : 1;

    #endregion

    #region Exposure time table (copy-pasted from color_priv.h)

    static int [] _exposureTimeTable50 =
      { 500, 1250, 2500, 10000, 20000, 30000 };

    static int [] _exposureTimeTable60 =
      { 500, 1250, 2500, 8330, 16670, 33330 };

    #endregion
}

}
