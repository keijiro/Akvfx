using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;

namespace Akvfx
{
    public sealed class SurfaceRenderer : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] Material _material = null;

        #endregion

        #region Internal objects

        Mesh _mesh;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _mesh = new Mesh();
            ConstructMesh();
        }

        void OnDestroy()
        {
            if (_mesh != null) Destroy(_mesh);
            _mesh = null;
        }

        void Update()
        {
            Graphics.DrawMesh(
                _mesh, transform.localToWorldMatrix,
                _material, gameObject.layer
            );
        }

        #endregion

        #region Mesh object operations

        void ConstructMesh()
        {
            const int columns = 640;
            const int rows = 576;

            using (var vertices = BuildVertexArray(columns, rows))
            {
                _mesh.SetVertexBufferParams(
                    vertices.Length,
                    new VertexAttributeDescriptor
                        (VertexAttribute.Position, VertexAttributeFormat.UInt8, 4),
                    new VertexAttributeDescriptor
                        (VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
                );
                _mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length);
            }

            using (var indices = BuildIndexArray(columns, rows))
            {
                _mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
                _mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
                _mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Length), MeshUpdateFlags.DontRecalculateBounds);
            }

            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
        }

        NativeArray<float3> BuildVertexArray(int columns, int rows)
        {
            var vertices = new NativeArray<float3>(
                columns * rows / 2,
                Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );

            var i = 0;

            for (var y = 0; y < rows; y++)
            {
                for (var x = y & 1; x < columns; x += 2)
                {
                    var u = (x + 0.5f) / columns;
                    var v = (y + 0.5f) / rows;
                    vertices[i++] = math.float3(0, u, v);
                }
            }

            return vertices;
        }

        NativeArray<int> BuildIndexArray(int columns, int rows)
        {
            var indices = new NativeArray<int>(
                (columns / 2 - 1) * 2 * 3 * (rows - 1),
                Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );

            var i = 0;
            var offs = 0;
            var next = columns / 2;

            for (var y = 0; y < rows - 1; y++)
            {
                for (var x = 0; x < columns / 2 - 1; x++)
                {
                    if ((y & 1) == 0)
                    {
                        indices[i++] = offs;
                        indices[i++] = offs + next;
                        indices[i++] = offs + 1;

                        indices[i++] = offs + next;
                        indices[i++] = offs + next + 1;
                        indices[i++] = offs + 1;
                    }
                    else
                    {
                        indices[i++] = offs;
                        indices[i++] = offs + next;
                        indices[i++] = offs + next + 1;

                        indices[i++] = offs;
                        indices[i++] = offs + next + 1;
                        indices[i++] = offs + 1;
                    }

                    offs++;
                }
                offs++;
            }

            return indices;
        }

        #endregion
    }
}
