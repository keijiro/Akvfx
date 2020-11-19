using Microsoft.Azure.Kinect.Sensor;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Akvfx {

//
// Per pixel ray direction table ("XY table") used to unproject depth
// samples to the 3D camera space
//
// This class directly invokes a native method (k4a_calibration_2d_to_3d)
// contained in libk4a to avoid double symbol definition problem between
// System.Numerics and System.Numerics.Vectors.
//
sealed class XYTable
{
    // Public property: Table data
    public ReadOnlySpan<float> Data => _data;

    // Float vvector types
    struct Float2 { public float x, y; }
    struct Float3 { public float x, y, z; }

    // Data storage
    float [] _data;

    // Constructor
    public XYTable(Calibration calibration, int width, int height)
    {
        // Data storage allocation
        var table = new float [width * height * 2];

        // XY table initialization
        // These loops could take a significant amount of time (e.g. 1 sec)
        // even on a decent desktop PC. Luckily, this can be easily
        // parallelized by using Parallel.For, so we did.
        Parallel.For(0, height, y => {
            Float2 v2;
            Float3 v3;
            bool isValid;

            v2.y = y;
            var offs = width * 2 * y;

            for (var x = 0; x < width; x++)
            {
                v2.x = x;

                k4a_calibration_2d_to_3d
                  (ref calibration,
                   ref v2, 1,
                   CalibrationDeviceType.Depth,
                   CalibrationDeviceType.Depth,
                   out v3,
                   out isValid);

                table[offs++] = v3.x;
                table[offs++] = v3.y;
            }
        });

        // Publish the table data.
        _data = table;
    }

    // k4a_calibration_2d_to_3d native method from libk4a
    [DllImport("k4a")]
    static extern int k4a_calibration_2d_to_3d
      ([In] ref Calibration calibration,
       ref Float2 source_point2d,
       float source_depth,
       CalibrationDeviceType source_camera,
       CalibrationDeviceType target_camera,
       out Float3 target_point3d,
       out bool valid);
}

}
