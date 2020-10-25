using UnityEngine;

/// <summary>
/// This class is used to retrieve Unity Engine's default meshes stored in the 
/// current project. It is also used to scale meshes.
/// </summary>
public static class PrimitiveMeshes
{
    /// <summary>
    /// The default Sphere mesh.
    /// </summary>
    public static Mesh Sphere { get { return Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx"); } }

    /// <summary>
    /// The default Capsule mesh.
    /// </summary>
    public static Mesh Capsule { get { return Resources.GetBuiltinResource<Mesh>("New-Capsule.fbx"); } }

    /// <summary>
    /// The default Cylinder mesh.
    /// </summary>
    public static Mesh Cylinder { get { return Resources.GetBuiltinResource<Mesh>("New-Cylinder.fbx"); } }

    /// <summary>
    /// The default Cube mesh.
    /// </summary>
    public static Mesh Cube { get { return Resources.GetBuiltinResource<Mesh>("Cube.fbx"); } }

    /// <summary>
    /// The default Plane mesh.
    /// </summary>
    public static Mesh Plane { get { return Resources.GetBuiltinResource<Mesh>("New-Plane.fbx"); } }

    /// <summary>
    /// The default Quad mesh.
    /// </summary>
    public static Mesh Quad { get { return Resources.GetBuiltinResource<Mesh>("Quad.fbx"); } }

    /// <summary>
    /// Retrieves the shared mesh of the given primitive type.
    /// </summary>
    /// <param name="type">The primitive type</param>
    /// <returns></returns>
    public static Mesh GetMesh(this PrimitiveType type)
    {
        switch(type)
        {
            case PrimitiveType.Sphere:
                return Sphere;
            case PrimitiveType.Capsule:
                return Capsule;
            case PrimitiveType.Cylinder:
                return Cylinder;
            case PrimitiveType.Cube:
                return Cube;
            case PrimitiveType.Plane:
                return Plane;
            case PrimitiveType.Quad:
                return Quad;
            default:
                return null;
        }
    }

    /// <summary>
    /// Returns a new scaled mesh without changing the original one.
    /// </summary>
    /// <param name="type">The primitive type with the mesh you want to scale</param>
    /// <param name="scale">The scale factor to apply to the mesh</param>
    /// <returns></returns>
    public static Mesh CreateScaledMesh(this PrimitiveType type, Vector3 scale)
    {
        return GetMesh(type).CreateScaledMesh(scale);
    }

    /// <summary>
    /// Returns a new scaled mesh without changing the original one.
    /// </summary>
    /// <param name="mesh">The mesh used to generate the new scaled one</param>
    /// <param name="scale">The scale factor to apply to the mesh</param>
    /// <returns></returns>
    public static Mesh CreateScaledMesh(this Mesh mesh, Vector3 scale)
    {
        Mesh scaledMesh = Object.Instantiate(mesh); // instantiated to avoid changing the original mesh

        var vertices = new Vector3[mesh.vertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = mesh.vertices[i];
            vertex.x *= scale.x;
            vertex.y *= scale.y;
            vertex.z *= scale.z;

            vertices[i] = vertex;
        }

        scaledMesh.vertices = vertices;

        scaledMesh.RecalculateNormals();
        scaledMesh.RecalculateBounds();

        return scaledMesh;
    }
}
