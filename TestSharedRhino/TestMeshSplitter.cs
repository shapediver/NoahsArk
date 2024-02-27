using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rhino.Geometry;
using NoahsArk.SharedRhino;
using System.Linq;

namespace NoahsArk.Tests.SharedRhino
{
    [TestClass]
    public class TestMeshSplitter
    {
        static IMeshSplitter MeshSplitter;

        /// <summary>
        /// Test setup for complete class, will be called once for all tests contained herein
        /// Change signature to "async static Task" in case of async tests
        /// </summary>
        /// <param name="context"></param>
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            MeshSplitter = new MeshSplitter();
        }

        /// <summary>
        /// Test setup per test, will be called once for each test
        /// Change signature to "async Task" in case of async tests
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            // TODO
        }

        /// <summary>
        /// Test method
        /// Change signature to "async Task" in case of async tests
        /// </summary>
        [TestMethod]
        public void Test_SplitAlongPlane_00()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1,1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            var plane = new Plane(new Point3d(0, 0, 0), new Vector3d(1, 1, 1)); // plane intersects mesh
            MeshSplitter.SplitAlongPlane(mesh, plane, out Mesh negMesh, out Mesh posMesh );

            Assert.AreEqual(mesh.Faces.Count, negMesh.Faces.Count + posMesh.Faces.Count, "Face count matches");

            foreach (var v in negMesh.Vertices)
            {
                Assert.IsTrue(0 > plane.DistanceTo(v), "Vertex contained in negative halfspace");
            }

            foreach (var f in posMesh.Faces)
            {
                Assert.IsFalse(MeshSplitter.IsFaceInNegativeHalfspace(posMesh, f, plane), "Face not contained in negative halfspace");
            }

            foreach (var f in negMesh.Faces)
            {
                Assert.IsTrue(MeshSplitter.IsFaceInNegativeHalfspace(negMesh, f, plane), "Face contained in negative halfspace");
            }
        }

        [TestMethod]
        public void Test_SplitAlongPlane_01()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            var plane = new Plane(new Point3d(-10, 0, 0), new Vector3d(1, 1, 1)); // plane on negative side of mesh
            MeshSplitter.SplitAlongPlane(mesh, plane, out Mesh negMesh, out Mesh posMesh);

            Assert.AreEqual(mesh.Faces.Count, negMesh.Faces.Count + posMesh.Faces.Count, "Face count matches");
            Assert.AreEqual(0, negMesh.Faces.Count, "Negative side mesh empty");
            Assert.AreEqual(0, negMesh.Vertices.Count, "Negative side mesh empty");

            foreach (var f in posMesh.Faces)
            {
                Assert.IsFalse(MeshSplitter.IsFaceInNegativeHalfspace(posMesh, f, plane), "Face not contained in negative halfspace");
            }
        }

        [TestMethod]
        public void Test_SplitAlongPlane_02()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            var plane = new Plane(new Point3d(10, 0, 0), new Vector3d(1, 1, 1)); // plane on positive side of mesh
            MeshSplitter.SplitAlongPlane(mesh, plane, out Mesh negMesh, out Mesh posMesh);

            Assert.AreEqual(mesh.Faces.Count, negMesh.Faces.Count + posMesh.Faces.Count, "Face count matches");
            Assert.AreEqual(0, posMesh.Faces.Count, "Positive side mesh empty");
            Assert.AreEqual(0, posMesh.Vertices.Count, "Positive side mesh empty");

            foreach (var v in negMesh.Vertices)
            {
                Assert.IsTrue(0 > plane.DistanceTo(v), "Vertex contained in negative halfspace");
            }

            foreach (var f in negMesh.Faces)
            {
                Assert.IsTrue(MeshSplitter.IsFaceInNegativeHalfspace(negMesh, f, plane), "Face contained in negative halfspace");
            }
        }

        [TestMethod]
        public void Test_SplitAlongPlanes_00()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            var planes = (new double[] { -0.5, 0, 0.5 }).Select( d => new Plane(new Point3d(d, 0, 0), new Vector3d(1, 1, 1))).ToArray();
            MeshSplitter.SplitAlongPlanes(mesh, planes, out Mesh[] meshes);

            Assert.AreEqual(planes.Length + 1, meshes.Length, "Number of planes + 1 matches number of meshes");

            Assert.AreEqual(mesh.Faces.Count, meshes.Sum(m => m.Faces.Count), "Face count matches");

            for (int i= 0; i < meshes.Length - 1; i++)
            {
                var m = meshes[i];
                for (int j=i; j< meshes.Length - 1; j++)
                {
                    var p = planes[j];
                    foreach (var v in m.Vertices)
                    {
                        Assert.IsTrue(0 > p.DistanceTo(v), "Vertex contained in negative halfspace");
                    }
                }
                for (int j = 0; j < i; j++)
                {
                    var p = planes[j];
                    foreach (var f in m.Faces)
                    {
                        Assert.IsFalse(MeshSplitter.IsFaceInNegativeHalfspace(m, f, p), "Face not contained in negative halfspace");
                    }
                }
            }
          
        }

        [TestMethod]
        public void Test_SplitAlongPlanes_01()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            var planes = (new double[] { -10, -0.5, 0, 0.5, 10 }).Select(d => new Plane(new Point3d(d, 0, 0), new Vector3d(1, 1, 1))).ToArray();
            MeshSplitter.SplitAlongPlanes(mesh, planes, out Mesh[] meshes);

            Assert.AreEqual(planes.Length + 1, meshes.Length, "Number of planes + 1 matches number of meshes");

            Assert.AreEqual(mesh.Faces.Count, meshes.Sum(m => m.Faces.Count), "Face count matches");
            Assert.AreEqual(0, meshes[0].Faces.Count, "First mesh empty");
            Assert.AreEqual(0, meshes[meshes.Length-1].Faces.Count, "Last mesh empty");

            for (int i = 0; i < meshes.Length - 1; i++)
            {
                var m = meshes[i];
                for (int j = i; j < meshes.Length - 1; j++)
                {
                    var p = planes[j];
                    foreach (var v in m.Vertices)
                    {
                        Assert.IsTrue(0 > p.DistanceTo(v), "Vertex contained in negative halfspace");
                    }
                }
                for (int j = 0; j < i; j++)
                {
                    var p = planes[j];
                    foreach (var f in m.Faces)
                    {
                        Assert.IsFalse(MeshSplitter.IsFaceInNegativeHalfspace(m, f, p), "Face not contained in negative halfspace");
                    }
                }
            }

        }

        [TestMethod]
        public void Test_SplitForMaxVertexCount_00()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            int maxVertexCount = 100;
            MeshSplitter.SplitForMaxVertexCount(mesh, out Mesh[] meshes, maxVertexCount);

            for (int i = 0; i < meshes.Length - 1; i++)
            {
                var m = meshes[i];
                Assert.IsTrue(m.Vertices.Count <= maxVertexCount, $"Vertex count less or equal to {maxVertexCount}");
            }

            Assert.AreEqual(mesh.Faces.Count, meshes.Sum(m => m.Faces.Count), "Face count matches");

        }

        [TestMethod]
        public void Test_SplitForMaxVertexCount_01()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            int maxVertexCount = 100000;
            MeshSplitter.SplitForMaxVertexCount(mesh, out Mesh[] meshes, maxVertexCount);

            for (int i = 0; i < meshes.Length - 1; i++)
            {
                var m = meshes[i];
                Assert.IsTrue(m.Vertices.Count <= maxVertexCount, $"Vertex count less or equal to {maxVertexCount}");
            }

            Assert.AreEqual(mesh.Faces.Count, meshes.Sum(m => m.Faces.Count), "Face count matches");

        }

        [TestMethod]
        public void Test_SplitForMaxVertexCount_02()
        {
            Mesh mesh = Mesh.CreateFromBox(new Box(Plane.WorldXY, new Interval(-1, 1), new Interval(-1, 1), new Interval(-1, 1)), 10, 10, 10);
            for (int i=0; i<mesh.Vertices.Count; i++)
                mesh.Vertices[i] = new Point3f(0, 0, 0);
            int maxVertexCount = 100000;
            MeshSplitter.SplitForMaxVertexCount(mesh, out Mesh[] meshes, maxVertexCount);

            for (int i = 0; i < meshes.Length - 1; i++)
            {
                var m = meshes[i];
                Assert.IsTrue(m.Vertices.Count <= maxVertexCount, $"Vertex count less or equal to {maxVertexCount}");
            }

            Assert.AreEqual(mesh.Faces.Count, meshes.Sum(m => m.Faces.Count), "Face count matches");

        }

        /// <summary>
        /// Test cleanup per test, will be called once for each test
        /// Change signature to "async Task" in case of async tests
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            // TODO 
        }

        /// <summary>
        /// Test cleanup for complete class, will be called once for all tests contained herein
        /// Change signature to "async static Task" in case of async tests
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            // TODO 
        }
    }
}