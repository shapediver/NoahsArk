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

        private void AddFaceToMesh(Mesh inmesh, MeshFace face, Mesh outmesh, int[] map)
        {
            if (map[face.A] == 0)
                map[face.A] = outmesh.Vertices.Add(inmesh.Vertices[face.A]);

            if (map[face.B] == 0)
                map[face.B] = outmesh.Vertices.Add(inmesh.Vertices[face.B]);

            if (map[face.C] == 0)
                map[face.C] = outmesh.Vertices.Add(inmesh.Vertices[face.C]);

            if (face.IsQuad && map[face.D] == 0)
                map[face.D] = outmesh.Vertices.Add(inmesh.Vertices[face.D]);

            if (face.IsQuad)
                outmesh.Faces.AddFace(map[face.A], map[face.B], map[face.C], map[face.D]);
            else
                outmesh.Faces.AddFace(map[face.A], map[face.B], map[face.C]);
        }

        public void SplitAlongPlane(Mesh mesh, Plane plane, out Mesh negMesh, out Mesh posMesh)
        {
            negMesh = new Mesh();
            posMesh = new Mesh();

            var negIndexMap = new int[mesh.Vertices.Count];
            var posIndexMap = new int[mesh.Vertices.Count];

            foreach (var face in mesh.Faces)
            {
                if (IsFaceInNegativeHalfspace(mesh, face, plane))
                    AddFaceToMesh(mesh, face, negMesh, negIndexMap);
                else
                    AddFaceToMesh(mesh, face, posMesh, posIndexMap);
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
            var indexMap = new int[planes.Length+1][]; // Note: This can become quite wasteful in terms of memory consumption

            var addFace = new Action<int, MeshFace>((i, face) =>
            {
                if (_meshes[i] == null)
                    _meshes[i] = new Mesh();
                if (indexMap[i] == null)
                    indexMap[i] = new int[mesh.Vertices.Count];
                AddFaceToMesh(mesh, face, _meshes[i], indexMap[i]);
            });

            foreach (var face in mesh.Faces)
            {
                bool added = false;
                for (int i=0; i < planes.Length; i++)
                {
                    var plane = planes[i];
                    if (IsFaceInNegativeHalfspace(mesh, face, plane))
                    {
                        addFace(i, face);
                        added = true;
                        break;
                    }
                }
                if (added)
                    continue;

                addFace(planes.Length, face);
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

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[sortedFaces[i].OriginalIndex];
                AddFaceToMesh(mesh, face, currentMesh, indexMap);
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