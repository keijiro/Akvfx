using UnityEngine;

namespace Akvfx {

public sealed class MaterialPropertyBinder : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] DeviceController _device = null;

    #endregion

    #region MonoBehaviour implementation

    Renderer _renderer;
    MaterialPropertyBlock _block;

    void Update()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_block == null) _block = new MaterialPropertyBlock();

        _renderer.GetPropertyBlock(_block);
        _block.SetTexture("_ColorMap", _device.ColorMap);
        _block.SetTexture("_PositionMap", _device.PositionMap);
        _renderer.SetPropertyBlock(_block);
    }

    #endregion
}

}
