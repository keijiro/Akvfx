using Microsoft.Azure.Kinect.Sensor;

namespace Akvfx
{
    sealed class DeviceSettingController
    {
        #region Public methods

        public DeviceSettingController(Device device, DeviceSettings initial)
        {
            ApplyInternal(device, initial, true);
        }

        public void ApplySettings(Device device, DeviceSettings settings)
        {
            ApplyInternal(device, settings, false);
        }

        #endregion

        #region Cached control values

        int _exposure;
        int _whiteBalance;
        int _brightness;
        int _contrast;
        int _saturation;
        int _sharpness;
        int _gain;
        int _blc;
        int _powerFreq;

        #endregion

        #region Private methods

        int ApplyControl(
            Device device, ColorControlCommand command,
            int newValue, int prevValue, bool forceApply
        )
        {
            // If nothing was changed, simply return the previous value.
            if (!forceApply && newValue == prevValue) return prevValue;

            // Apply the new value to the control.
            device.SetColorControl(
                command,
                newValue < 0 ? ColorControlMode.Auto : ColorControlMode.Manual,
                newValue < 0 ? 0 : newValue
            );
            return newValue;
        }

        void ApplyInternal(Device device, DeviceSettings settings, bool forceApply)
        {
            _exposure = ApplyControl(
                device, ColorControlCommand.ExposureTimeAbsolute,
                settings.ExposureDeviceValue, _exposure, forceApply
            );

            _whiteBalance = ApplyControl(
                device, ColorControlCommand.Whitebalance,
                settings.WhiteBalanceDeviceValue, _whiteBalance, forceApply
            );

            _brightness = ApplyControl(
                device, ColorControlCommand.Brightness,
                settings.BrightnessDeviceValue, _brightness, forceApply
            );

            _contrast = ApplyControl(
                device, ColorControlCommand.Contrast,
                settings.ContrastDeviceValue, _contrast, forceApply
            );

            _saturation = ApplyControl(
                device, ColorControlCommand.Saturation,
                settings.SaturationDeviceValue, _saturation, forceApply
            );

            _sharpness = ApplyControl(
                device, ColorControlCommand.Sharpness,
                settings.SharpnessDeviceValue, _sharpness, forceApply
            );

            // This is not documented, but the gain parameter can't update
            // while the auto exposure is enabled. To delay updates, we do a
            // bit tricky thing here.
            if (_exposure < 0 || forceApply)
                _gain = -1;
            else
                _gain = ApplyControl(
                    device, ColorControlCommand.Gain,
                    settings.GainDeviceValue, _gain, false
                );

            _blc = ApplyControl(
                device, ColorControlCommand.BacklightCompensation,
                settings.BlcDeviceValue, _blc, forceApply
            );

            _powerFreq = ApplyControl(
                device, ColorControlCommand.PowerlineFrequency,
                settings.PowerFreqDeviceValue, _powerFreq, forceApply
            );
        }

        #endregion
    }
}
