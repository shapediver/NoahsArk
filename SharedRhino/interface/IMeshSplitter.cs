using Rhino.Geometry;

namespace NoahsArk.SharedRhino
{
    
    /// <summary>
    /// Functionality for splitting large meshes into smaller parts.
    /// </summary>
    public interface IMeshSplitter
    {
        /// <summary>
        /// Test whether the given mesh face is fully contained in the negative half-space
        /// defined by the plane.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="face"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        bool IsFaceInNegativeHalfspace(Mesh mesh, MeshFace face, Plane plane);

        /// <summary>
        /// Split a mesh along a plane, such that all faces which are fully contained 
        /// in the negative half-space defined by the plane become part of the first
        /// resulting mesh, and all remaining faces become part of the second resulting mesh. 
        /// The remaining faces includes faces fully contained in the positive half-space 
        /// defined by the plane, and faces which are contained in both the negative 
        /// and the positive half-space. 
        /// </summary>
        /// <param name="mesh">The mesh to split</param>
        /// <param name="plane">The plane to split along</param>
        /// <param name="negativeSideMesh">Resulting mesh containing faces which are fully contained in the negative half-space of plane</param>
        /// <param name="positiveSideMesh">Resulting mesh containing faces which are NOT fully contained in the negative half-space of plane</param>
        /// <returns></returns>
        void SplitAlongPlane(Mesh mesh, Plane plane, out Mesh negativeSideMesh, out Mesh positiveSideMesh);

        /// <summary>
        /// Split a mesh along parallel planes. This is a generalisation of <see cref="SplitAlongPlane(Mesh, Plane, out Mesh, out Mesh)"/>
        /// The given planes are expected to be ordered by signed distance, i.e. the
        /// first plane's origin must be contained in the negative half-space of all other planes, the
        /// second plane's origin must be contained in the negative half-space of all planes starting from the third plane, etc.
        /// The planes need not necessarily be parallel, but they should not intersect within the bounding box of the given mesh, 
        /// otherwise the results might be unexpected. 
        /// Throws an ArgumentException in case the planes are not sorted as expected.
        /// </summary>
        /// <param name="mesh">The mesh to split</param>
        /// <param name="planes">the planes to split with</param>
        /// <param name="meshes">Resulting meshes</param>
        void SplitAlongPlanes(Mesh mesh, Plane[] planes, out Mesh[] meshes);

        /// <summary>
        /// Split a mesh into pieces such that each resulting mesh has a vertex count
        /// lower or equal to the given value. 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="meshes"></param>
        /// <param name="maxVertexCount"></param>
        void SplitForMaxVertexCount(Mesh mesh, out Mesh[] meshes, int maxVertexCount);
    }
    
}