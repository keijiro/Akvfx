using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Akvfx
{
    [AddComponentMenu("VFX/Property Binders/Akvfx/Point Cloud Binder")]
    [VFXBinder("Akvfx/Point Cloud")]
    sealed class VFXPointCloudBinder : VFXBinderBase
    {
        #region VFX Binder Implementation

        public string ColorMapProperty {
            get => (string)_colorMapProperty;
            set => _colorMapProperty = value;
        }

        public string PositionMapProperty {
            get => (string)_positionMapProperty;
            set => _positionMapProperty = value;
        }

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _colorMapProperty = "ColorMap";

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _positionMapProperty = "PositionMap";

        public DeviceController Target = null;

        public override bool IsValid(VisualEffect component)
          => Target != null &&
             component.HasTexture(_colorMapProperty) &&
             component.HasTexture(_positionMapProperty);

        public override void UpdateBinding(VisualEffect component)
        {
            if (Target.ColorMap == null) return;
            if (Target.PositionMap == null) return;
            component.SetTexture(_colorMapProperty, Target.ColorMap);
            component.SetTexture(_positionMapProperty, Target.PositionMap);
        }

        public override string ToString()
          => "Point Cloud : " + 
             $"'{_positionMapProperty}' -> {Target?.name ?? "(null)"}";

        #endregion
    }
}
