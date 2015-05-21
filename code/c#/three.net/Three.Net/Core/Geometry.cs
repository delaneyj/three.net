using System;
using System.Collections.Generic;
using System.Linq;
using Three.Net.Math;

namespace Three.Net.Core
{
    public class MorphTargetInfo 
    {
        public string Name;
        public List<Vector3> Vertices = new List<Vector3>();
    }

    public class MorphColorInfo
    {
        public string Name;
        public List<Color> Colors = new List<Color>();
    }

    public class MorphNormalsInfo
    {
        public class VertexNormalSet
        {
            public Vector3 A;
            public Vector3 B;
            public Vector3 C;
        }

        public List<VertexNormalSet> VertexNormals = new List<VertexNormalSet>();
    }

    public class GeometryGroup
    {
        private static uint nextId = 0;
        public readonly uint Id = nextId++;
        public List<int> FacesIndicies;
        public int VerticeCount;
        public int MorphTargetCount;
        public int MorphNormalsCount;
        public int? glVertexBuffer;
        public int? glNormalBuffer;
        public int? glTangentBuffer;
        public int? glColorBuffer;
        public int? glUVBuffer;
        public int? glUV2Buffer;
        public int? glSkinIndicesBuffer;
        public int? glSkinWeightsBuffer;
        public int? glFaceBuffer;
        public int? glLineBuffer;
        public int? glLineDistanceBuffer;
        public List<int> glMorphTargetsBuffers;
        public List<int> glMorphNormalsBuffers;
        public float[] vertexArray;
        public float[] normalArray;
        public float[] tangentArray;
        public float[] colorArray;
        public float[] lineDistanceArray;
        public float[] uvArray;
        public float[] uv2Array;
        public float[] skinIndexArray;
        public float[] skinWeightArray;
        public uint[] faceArray;
        public uint[] lineArray;
        public List<float[]> morphTargetsArrays;
        public List<float[]> morphNormalsArrays;
        public int glFaceCount;
        public int glLineCount;
        public List<object> glCustomAttributesList;
        public bool inittedArrays;
    }

    public class UVFaceSet
    {
        public Vector2 A, B, C;

        public UVFaceSet(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Vector2 this[int key]
        {
            get
            {
                switch(key)
                {
                    case 0: return A;
                    case 1: return B;
                    case 2: return C;
                    default: throw new InvalidOperationException();
                }
            }
            set
            {
                switch (key)
                {
                    case 0: A = value; break;
                    case 1: B = value; break;
                    case 2: C = value; break;
                    default: throw new InvalidOperationException();
                }
            }
        }
    }

    public class Geometry
    {
        private static ulong nextId= 0;

        public readonly ulong Id = nextId++;
        public Guid Guid = Guid.NewGuid();
        public string Name = string.Empty;

        public List<Vector3> vertices = new List<Vector3>();
        public List<Face3> faces = new List<Face3>();
        public List<List<UVFaceSet>> faceVertexUvs = new List<List<UVFaceSet>>();
        public List<Color> vertexColors;
        public bool glInit;
        public Sphere BoundingSphere = Sphere.Empty;
        public Box3 BoundingBox = Box3.Empty;

        internal List<GeometryGroup> groupsList;
        private Dictionary<string,GeometryGroup> groups;
        internal List<MorphColorInfo> MorphColors;
        internal List<MorphTargetInfo> MorphTargets = null;
        internal List<MorphNormalsInfo> MorphNormals = null;

        internal List<Vector4> SkinIndices;
        internal List<Vector4> SkinWeights;

	    // update flags
        internal bool hasTangents = false;
        internal bool dynamic = true; // the intermediate typed arrays will be deleted when set to false
	    internal bool VerticesNeedUpdate = false;
	    internal bool ElementsNeedUpdate = false;
	    internal bool UvsNeedUpdate = false;
	    internal bool NormalsNeedUpdate = false;
	    internal bool TangentsNeedUpdate = false;
	    internal bool ColorsNeedUpdate = false;
	    internal bool LineDistancesNeedUpdate = false;
        internal bool BuffersNeedUpdate = false;
        internal bool groupsNeedUpdate = false;
        public bool MorphTargetsNeedUpdate;

        public Geometry()
        {
            faceVertexUvs.Add(new List<UVFaceSet>());
        }

        public IEnumerable<GeometryGroup> Groups { get { return groupsList.AsEnumerable(); } }

        public void Apply(Matrix4 matrix ) {
            var normalMatrix = Matrix3.GetNormalMatrix( matrix );
            for(var i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[ i ];
			    vertex.Apply( matrix ); 
                vertices[i] = vertex;
            }

            for ( var i = 0; i < faces.Count; i ++ ) 
            {
			var face = faces[ i ];
			
                var a = face.NormalA;
                a.Apply(normalMatrix);
                a.Normalize();
                face.NormalA = a;

                var b = face.NormalB;
                b.Apply(normalMatrix);
                b.Normalize();
                face.NormalB = b;

                var c = face.NormalC;
                c.Apply(normalMatrix);
                c.Normalize();
                face.NormalC = c;
			}

		ComputeBoundingBox();
            ComputeBoundingSphere();
	}

        public void ComputeBoundingBox()
        {
            BoundingBox = Box3.FromPoints(vertices);
		}

        public void ComputeBoundingSphere()
        {
            BoundingSphere = Sphere.FromPoints(vertices);
        }

        public void ComputeTangents()
        {
            // based on http://www.terathon.com/code/tangent.html
            var tan1 = new List<Vector3>();
            var tan2 = new List<Vector3>();

            foreach (var v in vertices)
            {
                tan1.Add(Vector3.Zero);
                tan2.Add(Vector3.Zero);
            }

            for (var f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                var uv = faceVertexUvs[0][f]; // use UV layer 0 for tangents
                HandleTriangle(face.A, face.B, face.C, 0, 1, 2, uv, tan1, tan2);

            }

            foreach (var face in faces)
            {
                Vector3 temp, temp2 = Vector3.Zero;

                for (var i = 0; i < 3; i++)
                {
                    Vector3 n;
                    switch (i)
                    {
                        case 0: n = face.NormalA; break;
                        case 1: n = face.NormalB; break;
                        case 2: n = face.NormalC; break;
                        default: throw new NotSupportedException();
                    }

                    int vertexIndex;
                    switch (i)
                    {
                        case 0: vertexIndex = face.A; break;
                        case 1: vertexIndex = face.B; break;
                        case 2: vertexIndex = face.C; break;
                        default: throw new NotSupportedException();
                    }

                    var t = tan1[vertexIndex];

                    // Gram-Schmidt orthogonalize
                    var nTemp = n;
                    nTemp.Multiply(n.Dot(t));
                    temp = t;
                    temp.Subtract(nTemp);
                    temp.Normalize();

                    // Calculate handedness
                    temp2 = Vector3.CrossVectors(n, t);
                    var test = temp2.Dot(tan2[vertexIndex]);
                    var w = (test < 0) ? -1 : 1;

                    var v4 = new Vector4(temp.x, temp.y, temp.z, w);
                    switch (i)
                    {
                        case 0: face.TangentA = v4; break;
                        case 1: face.TangentB = v4; break;
                        case 2: face.TangentC = v4; break;
                        default: throw new NotSupportedException();
                    }
                }
            }
        }

        private void HandleTriangle(int a, int b, int c, int ua, int ub, int uc, UVFaceSet uv, List<Vector3> tan1, List<Vector3> tan2)
        {
            Vector3 vA = vertices[a], vB = vertices[b], vC = vertices[c];
            Vector2 uvA = uv[ua], uvB = uv[ub], uvC = uv[uc];
            var x1 = vB.x - vA.x;
            var x2 = vC.x - vA.x;
            var y1 = vB.y - vA.y;
            var y2 = vC.y - vA.y;
            var z1 = vB.z - vA.z;
            var z2 = vC.z - vA.z;
            var s1 = uvB.x - uvA.x;
            var s2 = uvC.x - uvA.x;
            var t1 = uvB.y - uvA.y;
            var t2 = uvC.y - uvA.y;
            var r = 1 / (s1 * t2 - s2 * t1);

            var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            var tA1 = tan1[a];
            tA1.Add(sdir);
            tan1[a] = tA1;

            var tB1 = tan1[b];
            tB1.Add(sdir);
            tan1[b] = tB1;

            var tC1 = tan1[c];
            tC1.Add(sdir);
            tan1[c] = tC1;

            var tA2 = tan2[a];
            tA2.Add(tdir);
            tan2[a] = tA2;

            var tB2 = tan2[b];
            tB2.Add(tdir);
            tan2[b] = tB2;

            var tC2 = tan2[c];
            tC2.Add(tdir);
            tan2[c] = tC2;
        }

        public void ComputeNormals()
        {
            // vertex normals weighted by triangle areas
            // http://www.iquilezles.org/www/articles/normals/normals.htm
            Vector3 cb = Vector3.Zero, ab = Vector3.Zero;

            foreach(var face in faces)
            {
                var vA = vertices[face.A];
                var vB = vertices[face.B];
                var vC = vertices[face.C];

                cb = Vector3.SubtractVectors(vC, vB);
                ab = Vector3.SubtractVectors(vA, vB);
                cb.Cross(ab);

                vA.Add(cb);
                vB.Add(cb);
                vC.Add(cb);

                face.NormalA = vA.Normalized();
                face.NormalB = vB.Normalized();
                face.NormalC = vC.Normalized();
            }
        }

        public int MergeVertices()
        {
            var verticesMap = new Dictionary<string, int>();
            var unique = new List<Vector3>();
            var changes = new List<int>();
            var precisionPoints = 4; // number of decimal points, eg. 4 for epsilon of 0.0001
            var precision = Mathf.Pow(10, precisionPoints);

            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                var x = Mathf.Round(v.x * precision);
                var y = Mathf.Round(v.y * precision);
                var z = Mathf.Round(v.z * precision);
                var key = string.Format("{0}_{1}_{2}", x, y, z);
                
                if (changes.Count == i) changes.Add(0);

                if (verticesMap.ContainsKey(key))
                {
                    //Debug.WriteLine("Duplicate vertex found. {0}  could be using {1}.", i, verticesMap[key]);
                    changes[i] = changes[verticesMap[key]];
                }
                else
                {
                    verticesMap.Add(key, i);
                    unique.Add(v);
                    changes[i] = unique.Count - 1;
                }
            }

            // if faces are completely degenerate after merging vertices, we
            // have to remove them from the geometry.
            var faceIndicesToRemove = new List<int>();

            for (var i = 0; i < faces.Count; i++)
            {
                var face = faces[i];
                face.A = changes[face.A];
                face.B = changes[face.B];
                face.C = changes[face.C];

                var indices = new int[] { face.A, face.B, face.C };
                var dupIndex = -1;

                // if any duplicate vertices are found in a Face3
                // we have to remove the face as nothing can be saved
                for (var n = 0; n < 3; n++)
                {
                    if (indices[n] == indices[(n + 1) % 3])
                    {
                        dupIndex = n;
                        faceIndicesToRemove.Add(i);
                        break;
                    }
                }
            }

            for (var i = faceIndicesToRemove.Count - 1; i >= 0; i--)
            {
                var idx = faceIndicesToRemove[i];
                faces.RemoveAt(idx);

                for (var j = 0; j < faceVertexUvs.Count; j++)
                {
                    faceVertexUvs[j].RemoveAt(idx);
                }
            }

            // Use unique set of vertices

            var diff = vertices.Count - unique.Count;
            vertices = unique;
            return diff;
        }

        private class GroupingInfo
        {
            public int Hash;
            public int Counter;
        }

        // Geometry splitting
        public void MakeGroups(uint maxVerticesInGroup = uint.MaxValue)
        {
            var materialIndex = 0;

            var hashMap = new Dictionary<int,GroupingInfo>();
            var numMorphTargets = MorphTargets != null ? MorphTargets.Count : 0;
			var numMorphNormals = MorphNormals != null ? MorphNormals.Count : 0;

            groups = new Dictionary<string,GeometryGroup>();
            groupsList = new List<GeometryGroup>();
			
            for(var f = 0; f < faces.Count; f++)
            {
                var face = faces[f];

                if(!hashMap.ContainsKey(materialIndex))
                {
                    //Hash & Counter
                    hashMap.Add(materialIndex, new GroupingInfo()
                    {
                        Hash = materialIndex,
                        Counter = 0
                    });
				}

                var info = hashMap[materialIndex];
                var groupHash = string.Format("{0}_{1}", info.Hash, info.Counter);

                GeometryGroup group;
                if(!groups.TryGetValue(groupHash, out group))
                {
                    group = new GeometryGroup()
                    {
                        FacesIndicies = new List<int>(),
                        VerticeCount = 0,
                        MorphTargetCount = numMorphTargets,
                        MorphNormalsCount = numMorphNormals
                    };
                    
					groups.Add(groupHash,group);
					groupsList.Add(group);
				}

				if (group.VerticeCount + 3 > maxVerticesInGroup ) 
                {
                    groupHash = string.Format("{0}_{1}", info.Hash, ++info.Counter);

					if(groups.TryGetValue(groupHash, out group))
                    {
						group = new GeometryGroup()
                        {
                            FacesIndicies = new List<int>(),
                            VerticeCount = 0, 
                            MorphTargetCount = numMorphTargets, 
                            MorphNormalsCount = numMorphNormals 
                        };
						groups[ groupHash ] = group;
						groupsList.Add(group);	
					}
				}

                group.FacesIndicies.Add(f);
                group.VerticeCount += 3;
			}
        }
    }
}
