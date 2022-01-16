using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OptimizePolygon : MonoBehaviour
{
    [System.Flags]
    enum EOption
    {
        None = 0,
        SufaceNormal = 1,
        SmoothNormal = 2
    }

    [System.Serializable]
    struct OptionFlags
    {
        [SerializeField]
        EOption m_flags;
        static readonly List<EOption> flags = new List<EOption>(System.Enum.GetValues(typeof(OptimizePolygon.EOption)).Cast<EOption>());

        public EOption Flags => m_flags;
        public int Value
        {
            get
            {
                int value = 0;
                foreach (var o in flags)
                {
                    if ((m_flags & o) > 0)
                    {
                        value += (int)o;
                    }
                }
                return value;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    [SerializeField]
    MeshFilter m_meshFilter;
    [SerializeField]
    OptionFlags m_optionFlags;

    static readonly Dictionary<Mesh, Dictionary<int, Mesh>> m_optimizeMeshMap = new Dictionary<Mesh, Dictionary<int, Mesh>>();

    void Awake()
    {
        if (m_meshFilter == null) m_meshFilter = GetComponent<MeshFilter>();
    }

    // Start is called before the first frame update
    void Start()
    {
        int option = m_optionFlags.Value;
        if (option == 0 || m_meshFilter == null) return;
        Mesh mesh = m_meshFilter.sharedMesh;

        if (!m_optimizeMeshMap.ContainsKey(mesh))
        {
            m_optimizeMeshMap.Add(mesh, new Dictionary<int, Mesh>());
        }
        else
        {
            if (m_optimizeMeshMap[mesh].ContainsKey(option))
            {
                m_meshFilter.sharedMesh = m_optimizeMeshMap[mesh][option];
                return;
            }
        }

        Mesh newMesh = new Mesh();

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector2[] uvs = mesh.uv;
        
        List<Vector3> newNormals = new List<Vector3>(normals);
        List<Vector4> newTangents = new List<Vector4>();
        newTangents.Resize(vertices.Length);
        //List<bool> isVerticesFlat = new List<bool>(vertices.Length);
        //Dictionary<Vector3, int> vertexIndex = new Dictionary<Vector3, int>();
        //Dictionary<int, Dictionary<int, int>> nearIndex = new Dictionary<int, Dictionary<int, int>>();

        Dictionary<Vector3, List<int>> sameVertices = new Dictionary<Vector3, List<int>>();
        Dictionary<Vector3, HashSet<Vector3>> vertexNormalMap = new Dictionary<Vector3, HashSet<Vector3>>();
        Dictionary<Vector3, List<int>> nearVertexIndex = new Dictionary<Vector3, List<int>>();
        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            //isVerticesFlat.Clear();
            //vertexIndex.Clear();
            //nearIndex.Clear();
        
            // Surface Normal
            int[] indicies = mesh.GetTriangles(i);
            for (int idx = 0; idx < indicies.Length; idx += 3)
            {
                int vtxIndex;
                Vector3 sufNormal = Vector3.Cross(vertices[indicies[idx + 1]] - vertices[indicies[idx]], vertices[indicies[idx + 2]] - vertices[indicies[idx + 1]]).normalized;
                for (int seq = 0; seq < 3; ++seq)
                {
                    vtxIndex = indicies[idx + seq];
                    newNormals[vtxIndex] = sufNormal;

                    // same vertex link
                    if (!sameVertices.ContainsKey(vertices[vtxIndex])) { sameVertices.Add(vertices[vtxIndex], new List<int>()); }
                    sameVertices[vertices[vtxIndex]].Add(vtxIndex);

                    // vertex normals
                    if (!vertexNormalMap.ContainsKey(vertices[vtxIndex])) { vertexNormalMap.Add(vertices[vtxIndex], new HashSet<Vector3>()); }
                    
                    if ((m_optionFlags.Flags & EOption.SufaceNormal) > 0)
                    {
                        // Surface Normal
                        vertexNormalMap[vertices[vtxIndex]].Add(sufNormal);
                    }
                    else
                    {
                        // Mesh Normal
                        vertexNormalMap[vertices[vtxIndex]].Add(normals[vtxIndex]);
                    }

                    // near vertex
                    if (!nearVertexIndex.ContainsKey(vertices[vtxIndex])) { nearVertexIndex.Add(vertices[vtxIndex], new List<int>()); }
                    nearVertexIndex[vertices[vtxIndex]].Add(indicies[idx + (seq + 1) % 3]);
                    nearVertexIndex[vertices[vtxIndex]].Add(indicies[idx + (seq + 2) % 3]);
                }
            }
        }

        if ((m_optionFlags.Flags & EOption.SmoothNormal) > 0)
        {
            // Smooth Normal
            foreach (var sv in sameVertices)
            {
                Vector3 tangent = Vector3.zero;
                float w = 1 / nearVertexIndex[sv.Key].Count;
                foreach (var near in nearVertexIndex[sv.Key])
                {
                    tangent += (sv.Key - vertices[near]).normalized;
                }

                Vector3 smoothNormal = Vector3.zero;
                foreach (var normal in vertexNormalMap[sv.Key])
                {
                    smoothNormal += normal;
                }
                smoothNormal = smoothNormal.normalized;

                foreach (var idx in sv.Value)
                {
                    newNormals[idx] = smoothNormal;
                    Vector4 newTangent = Vector4.Normalize(tangent.normalized);
                    newTangent.w = tangent.magnitude * w;
                    newTangents[idx] = newTangent;
                }
            }
        }
        
        newMesh.vertices = vertices;
        newMesh.normals = newNormals.ToArray();
        newMesh.tangents = newTangents.ToArray();
        newMesh.uv = uvs;
        newMesh.name = m_optionFlags.ToString() + mesh.name;
        for (int i = 0; i < mesh.subMeshCount; ++i)
        {
            newMesh.SetIndices(mesh.GetTriangles(i), MeshTopology.Triangles, i);
        }

        m_optimizeMeshMap[mesh].Add(option, newMesh);
        m_meshFilter.sharedMesh = newMesh;
    }
}
