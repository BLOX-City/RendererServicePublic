using Assimp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BLOXCityRenderer.Data;

public class Mesh
{
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> texCoords = new List<Vector2>();
    public List<Vector3> colors = new List<Vector3>();
    public List<uint> indices = new List<uint>();
    public int VertexArrayObject { get; set; }
    public int VerticeBufferObject { get; set; }
    public int NormalBufferObject { get; set; }
    public int VertexColorBufferObject { get; set; }
    public int TextureCoordBufferObject { get; set; }
    public int ElementBufferObject { get; set; }
    public Texture? Texture { get; set; }
    public bool HasNoLighting { get; set; }
    public uint Color { get; set; } = 0xffffff;
    public Vector3 Position { get; set; } = new Vector3();
    public void AssimpMeshLoad(Assimp.Mesh mesh)
    {
        for (int i = 0; i < mesh.Vertices.Count; i++)
        {
            vertices.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
        }

        for (int i = 0; i < mesh.Normals.Count; i++)
            normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));

        for (int i = 0; i < mesh.TextureCoordinateChannels[0].Count; i++)
        {
            texCoords.Add(new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
        }

        /*for(int i = 1; i < mesh.Faces.Count; i++) {
            Assimp.Face face = mesh.Faces[i];
            indices.Add((uint)face.Indices[1]);
            indices.Add((uint)face.Indices[2]);
            indices.Add((uint)face.Indices[0]);
            
        }*/
        int[] indices = mesh.GetIndices();
        for (int i = 0; i < indices.Length; i++)
            this.indices.Add((uint)indices[i]);

        bool hasVertexColors = mesh.HasVertexColors(0);

        if (hasVertexColors)
        {
            var color = mesh.VertexColorChannels[0];

            for (int i = 0; i < color.Count; i++)
                colors.Add(new Vector3(color[i].R, color[i].G, color[i].B));
        }




    }
    public void Initialize()
    {
        VertexArrayObject = GL.GenVertexArray();
        VerticeBufferObject = GL.GenBuffer();
        ElementBufferObject = GL.GenBuffer();
        NormalBufferObject = GL.GenBuffer();
        TextureCoordBufferObject = GL.GenBuffer();
        VertexColorBufferObject = GL.GenBuffer();
        
        GL.BindVertexArray(VertexArrayObject);

        GL.BindBuffer(BufferTarget.ArrayBuffer, VerticeBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vector3.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);


        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), indices.ToArray(), BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ArrayBuffer, NormalBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, normals.Count * Vector3.SizeInBytes, normals.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(2);

        GL.BindBuffer(BufferTarget.ArrayBuffer, TextureCoordBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Count * Vector2.SizeInBytes, texCoords.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);
        GL.EnableVertexAttribArray(3);

        if (colors.Count > 0) {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexColorBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, colors.Count * Vector3.SizeInBytes, colors.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(4);        
        }


    }
    public void LoadFromFile(string path)
    {
        AssimpContext assimpContext = new AssimpContext();
        Assimp.Scene scene = assimpContext.ImportFile(path, PostProcessSteps.Triangulate);

        if (scene.MeshCount == 0) throw new Exception("Mesh count cannot be 0");
        Assimp.Mesh mesh = scene.Meshes[0];
        AssimpMeshLoad(mesh);
    }

    public void LoadFromData(Stream stream)
    {
        AssimpContext assimpContext = new AssimpContext();
        Assimp.Scene scene = assimpContext.ImportFileFromStream(stream, PostProcessSteps.Triangulate);

        if (scene.MeshCount == 0) throw new Exception("Mesh count cannot be 0");
        Assimp.Mesh mesh = scene.Meshes[0];
        AssimpMeshLoad(mesh);
    }

    public void Draw()
    {
        GL.BindVertexArray(VertexArrayObject);
        GL.PointSize(5.0f);
        GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);
    }

    public Mesh()
    {

    }

    public Mesh(string filePath, Texture texture)
    {
        this.LoadFromFile(filePath);
        Texture = texture;
    }
    
    public Mesh(string filePath)
    {
        this.LoadFromFile(filePath);
    }    
}