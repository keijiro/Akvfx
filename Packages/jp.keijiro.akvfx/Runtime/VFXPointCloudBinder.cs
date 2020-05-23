using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Akvfx {

[AddComponentMenu("VFX/Property Binders/Akvfx/Point Cloud Binder")]
[VFXBinder("Akvfx/Point Cloud")]
sealed class VFXPointCloudBinder : VFXBinderBase
{
    #region VFX Binder Implementation

    public string ColorMapProperty
      { get => (string)_colorMapProperty;
        set => _colorMapProperty = value; }

    public string PositionMapProperty
      { get => (string)_positionMapProperty;
        set => _positionMapProperty = value; }

    public string WidthProperty
      { get => (string)_widthProperty;
        set => _widthProperty = value; }

    public string HeightProperty
      { get => (string)_heightProperty;
        set => _heightProperty = value; }

    [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
    ExposedProperty _colorMapProperty = "ColorMap";

    [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
    ExposedProperty _positionMapProperty = "PositionMap";

    [VFXPropertyBinding("System.UInt32"), SerializeField]
    ExposedProperty _widthProperty = "Width";

    [VFXPropertyBinding("System.UInt32"), SerializeField]
    ExposedProperty _heightProperty = "Height";

    public DeviceController Target = null;

    public override bool IsValid(VisualEffect component)
      => Target != null &&
         component.HasTexture(_colorMapProperty) &&
         component.HasTexture(_positionMapProperty) &&
         component.HasUInt(_widthProperty) &&
         component.HasUInt(_heightProperty);

    public override void UpdateBinding(VisualEffect component)
    {
        if (Target.ColorMap == null) return;
        if (Target.PositionMap == null) return;
        component.SetTexture(_colorMapProperty, Target.ColorMap);
        component.SetTexture(_positionMapProperty, Target.PositionMap);
        component.SetUInt(_widthProperty, (uint)ThreadedDriver.ImageWidth);
        component.SetUInt(_heightProperty, (uint)ThreadedDriver.ImageHeight);
    }

    public override string ToString()
      => "Point Cloud : " + 
         $"'{_positionMapProperty}' -> {Target?.name ?? "(null)"}";

    #endregion
}

}
