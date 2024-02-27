using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace NoahsArk.SharedRhino
{
    
   
    public class MeshSplitter : IMeshSplitter 
    {
       
        public bool IsFaceInNegativeHalfspace(Mesh mesh, MeshFace face, Plane plane)
        {
            var v = mesh.Vertices;
            if (plane.DistanceTo(v[face.A]) >= 0)
                return false;
            if (plane.DistanceTo(v[face.B]) >= 0)
                return false;
            if (plane.DistanceTo(v[face.C]) >= 0)
                return false;
            if (face.IsQuad && plane.DistanceTo(v[face.D]) >= 0)
                return false;
            return true;
        }

        private void AddFaceToMesh(Mesh inmesh, int faceIndex, Mesh outmesh, int[] map, bool copyVertexNormals, bool copyVertexColors, bool copyFaceNormals)
        {
            var addVertex = new Action<int>((idx) =>
            {
                map[idx] = outmesh.Vertices.Add(inmesh.Vertices[idx]);
                if (copyVertexNormals)
                    outmesh.Normals.Add(inmesh.Normals[idx]);
                if (copyVertexColors)
                    outmesh.VertexColors.Add(inmesh.VertexColors[idx]);
            });

            var face = inmesh.Faces[faceIndex];

            if (map[face.A] == 0)
                addVertex(face.A);
        
            if (map[face.B] == 0)
                addVertex(face.B);

            if (map[face.C] == 0)
                addVertex(face.C);

            if (face.IsQuad && map[face.D] == 0)
                addVertex(face.D);

            if (face.IsQuad)
            {
                outmesh.Faces.AddFace(map[face.A], map[face.B], map[face.C], map[face.D]);
                if (copyFaceNormals)
                    outmesh.FaceNormals.AddFaceNormal(inmesh.FaceNormals[faceIndex]);
            }
            else
            {
                outmesh.Faces.AddFace(map[face.A], map[face.B], map[face.C]);
                if (copyFaceNormals)
                    outmesh.FaceNormals.AddFaceNormal(inmesh.FaceNormals[faceIndex]);
            }
        }

        public void SplitAlongPlane(Mesh mesh, Plane plane, out Mesh negMesh, out Mesh posMesh)
        {
            negMesh = new Mesh();
            posMesh = new Mesh();

            var negIndexMap = new int[mesh.Vertices.Count];
            var posIndexMap = new int[mesh.Vertices.Count];

            bool copyVertexNormals = mesh.Normals.Count == mesh.Vertices.Count;
            bool copyVertexColors = mesh.VertexColors.Count == mesh.Vertices.Count;
            bool copyFaceNormals = mesh.FaceNormals.Count == mesh.Faces.Count;

            for (int faceIndex = 0; faceIndex < mesh.Faces.Count; faceIndex++)
            {
                var face = mesh.Faces[faceIndex];
                if (IsFaceInNegativeHalfspace(mesh, face, plane))
                    AddFaceToMesh(mesh, faceIndex, negMesh, negIndexMap, copyVertexNormals, copyVertexColors, copyFaceNormals);
                else
                    AddFaceToMesh(mesh, faceIndex, posMesh, posIndexMap, copyVertexNormals, copyVertexColors, copyFaceNormals);
            }
        }

        public void SplitAlongPlanes(Mesh mesh, Plane[] planes, out Mesh[] meshes)
        {
            meshes = null;

            // basic check whether plane sorting requirement is fulfilled
            for (int i=1; i<planes.Length; i++)
            {
                var p1 = planes[i-1];
                var p2 = planes[i];
                if (p2.DistanceTo(p1.Origin) >= 0)
                {
                    throw new ArgumentException("Planes are not sorted as expected");
                }
            }

            var _meshes = new Mesh[planes.Length+1];
            for (int i=0; i<_meshes.Length; i++)
                _meshes[i] = new Mesh();
            var indexMap = new int[planes.Length+1][]; // Note: This can become quite wasteful in terms of memory consumption

            bool copyVertexNormals = mesh.Normals.Count == mesh.Vertices.Count;
            bool copyVertexColors = mesh.VertexColors.Count == mesh.Vertices.Count;
            bool copyFaceNormals = mesh.FaceNormals.Count == mesh.Faces.Count;

            var addFace = new Action<int, int>((meshIndex, faceIndex) =>
            {
                if (indexMap[meshIndex] == null)
                    indexMap[meshIndex] = new int[mesh.Vertices.Count];
                AddFaceToMesh(mesh, faceIndex, _meshes[meshIndex], indexMap[meshIndex], copyVertexNormals, copyVertexColors, copyFaceNormals);
            });

            for (int faceIndex = 0; faceIndex < mesh.Faces.Count; faceIndex++)
            {
                var face = mesh.Faces[faceIndex];
                bool added = false;
                for (int planeIndex=0; planeIndex < planes.Length; planeIndex++)
                {
                    var plane = planes[planeIndex];
                    if (IsFaceInNegativeHalfspace(mesh, face, plane))
                    {
                        addFace(planeIndex, faceIndex);
                        added = true;
                        break;
                    }
                }
                if (added)
                    continue;

                addFace(planes.Length, faceIndex);
            }

            meshes = _meshes;
        }

        public struct IndexedNumber
        {
            public double Number;
            public int OriginalIndex;

            public IndexedNumber(double number, int index)
            {
                Number = number;
                OriginalIndex = index;
            }
        }


        public void SplitForMaxVertexCount(Mesh mesh, out Mesh[] _meshes, int maxVertexCount)
        {
            // get bounding box
            var bbox = mesh.GetBoundingBox(false);

            // sort faces along bounding box diagonal
            var dir = bbox.Max - bbox.Min; 
            dir.Unitize();
            if (!dir.IsValid)
                dir = Vector3d.XAxis;

            var sortedFaces = new List<IndexedNumber>(mesh.Faces.Count);
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[i];
                var pt = mesh.Vertices[face.A];
                sortedFaces.Add(new IndexedNumber(Vector3d.Multiply(dir, new Vector3d(pt)), i));
            }
            sortedFaces.Sort((a,b) => a.Number.CompareTo(b.Number));

            var meshes = new List<Mesh>();
            var currentMesh = new Mesh();
            var indexMap = new int[mesh.Vertices.Count];

            bool copyVertexNormals = mesh.Normals.Count == mesh.Vertices.Count;
            bool copyVertexColors = mesh.VertexColors.Count == mesh.Vertices.Count;
            bool copyFaceNormals = mesh.FaceNormals.Count == mesh.Faces.Count;

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var faceIndex = sortedFaces[i].OriginalIndex;
                AddFaceToMesh(mesh, faceIndex, currentMesh, indexMap, copyVertexNormals, copyVertexColors, copyFaceNormals);
                if (currentMesh.Vertices.Count + 4 > maxVertexCount)
                {
                    meshes.Add(currentMesh);
                    currentMesh = new Mesh();
                    indexMap = new int[mesh.Vertices.Count];
                }
            }

            if (currentMesh.Faces.Count > 0)
                meshes.Add(currentMesh);
        
            _meshes = meshes.ToArray();
        }

    }

}