using System.Collections.Concurrent;
using BLOXCityRenderer.Data;
using BLOXCityRenderer.Objects;
using BLOXCityRenderer.Utils;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BLOXCityRenderer;

public enum CharacterPart
{
    Head = 0,

    Torso = 1,
    LeftArm = 2,
    LeftHand = 3,
    RightHand = 4,
    RightArm = 5,
    LeftLeg = 6,
    LeftFoot = 7,
    RightLeg = 8,
    RightFoot = 9
}
public static class Renderer
{
    public static Mesh Test = new Mesh();
    static ShaderProgram testShader = new ShaderProgram();
    public static World World = new World();
    private static List<Mesh> _meshCache = new List<Mesh>();
    public static List<Texture> textures = new List<Texture>();
    public static TaskCompletionSource RenderingTask = new TaskCompletionSource();
    public static ConcurrentStack<RenderOperation> RenderOperations = new ConcurrentStack<RenderOperation>();
    public static ConcurrentDictionary<Guid, TaskCompletionSource<SkiaSharp.SKBitmap>> Rendered = new ConcurrentDictionary<Guid, TaskCompletionSource<SkiaSharp.SKBitmap>>();
    private static int _protectedMeshIndex;
    private static int _protectedTextureIndex;
    private static Texture _errorTexture = default!;
    public static void SetFullbodyCamera()
    {
        Renderer.World.ActiveCamera!.Rotation = new Vector3(379.79922f, 370.79922f, 0);
        Renderer.World.ActiveCamera.Position = new Vector3(0.8740163f, -3.7181618f, -3.7042923f);        
    }
    public static void SetHeadshotCamera()
    {
        Renderer.World.ActiveCamera!.Rotation = new Vector3(359.79922f, 370.79922f, 0);
        Renderer.World.ActiveCamera.Position = new Vector3(0.6637526f, -4.1208575f, -3.391686f);
    }
    public static void Initialize(int width, int height)
    {
        Camera camera = new Camera(new Vector2(width, height));
        camera.GenerateProjection();
        int cameraIndex = World.AddCamera(camera);
        World.MakeActiveCamera(cameraIndex);

        var shirt = new Texture();
        var pants = new Texture();
        var face = new Texture();

        _errorTexture = new Texture("assets/textures/error.jpeg");

        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/head.obj"));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/torso.obj", shirt));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/left_arm.obj", shirt));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/left_hand.obj"));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/right_hand.obj"));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/right_arm.obj", shirt));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/left_leg.obj", pants));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/left_foot.obj", pants));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/right_leg.obj", pants));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/right_foot.obj", pants));
        World.Game["Meshes"].Add(new Mesh("assets/mesh/character_parts/face.obj", face)
        {
            HasNoLighting = true
        });

        _protectedMeshIndex = World.Game["Meshes"].Count;

        face.InitFromFile("assets/textures/avatars_default/face.png");
        shirt.InitFromFile("assets/textures/staging/shirt_template2.png");
        pants.InitFromFile("assets/textures/staging/jeans.png");

        textures.Add(face);
        textures.Add(pants);
        textures.Add(shirt);

        _protectedTextureIndex = textures.Count;
        // textures.Add(new Texture("assets/textures/avatars_default/face.png"));

        testShader.Vertex = new Shader("./assets/shaders/default.vs");
        testShader.Vertex.ShaderType = ShaderType.VertexShader;
        testShader.Fragment = new Shader("./assets/shaders/diffuse.fs");
        testShader.Fragment.ShaderType = ShaderType.FragmentShader;
        testShader.Vertex.Compile();
        testShader.Fragment.Compile();
        testShader.Compile();
        testShader.Use();

        Test.Initialize();
        foreach (object mesh in World.Game["Meshes"])
        {
            Mesh m = (Mesh)mesh;
            m.Initialize();
        }
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.Blend);
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.AlphaTest);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.Texture2D);
    }

    public static void LoadShirt(string path)
    {

        Texture texture;

        try
        {
            texture = new Texture(path);

            textures.Add(texture);
        }
        catch (Exception)
        {
            texture = _errorTexture;
        }

        ((Mesh)World.Game["Meshes"][1]).Texture = texture;
        ((Mesh)World.Game["Meshes"][2]).Texture = texture;
        ((Mesh)World.Game["Meshes"][5]).Texture = texture;
    }

    public static void LoadPants(string path)
    {

        Texture texture;

        try
        {
            texture = new Texture(path);

            textures.Add(texture);
        }
        catch (Exception)
        {
            texture = _errorTexture;
        }

        ((Mesh)World.Game["Meshes"][6]).Texture = texture;
        ((Mesh)World.Game["Meshes"][7]).Texture = texture;
        ((Mesh)World.Game["Meshes"][8]).Texture = texture;
        ((Mesh)World.Game["Meshes"][9]).Texture = texture;
    }

    public static void ResetShirt()
    {
        ((Mesh)World.Game["Meshes"][1]).Texture = null;
        ((Mesh)World.Game["Meshes"][2]).Texture = null;
        ((Mesh)World.Game["Meshes"][5]).Texture = null;
    }

    public static void ResetPants()
    {
        ((Mesh)World.Game["Meshes"][6]).Texture = null;
        ((Mesh)World.Game["Meshes"][7]).Texture = null;
        ((Mesh)World.Game["Meshes"][8]).Texture = null;
        ((Mesh)World.Game["Meshes"][9]).Texture = null;
    }
    public static void SetColor(CharacterPart characterPart, uint color)
    {
        if (characterPart == CharacterPart.Head)
        {
            ((Mesh)World.Game["Meshes"][(int)characterPart]).Color = color;
            // ((Mesh)World.Game["Meshes"][10]).Color = color;
        }

        if (characterPart == CharacterPart.Torso)
        {
            ((Mesh)World.Game["Meshes"][(int)characterPart]).Color = color;
        }

        if (characterPart == CharacterPart.LeftArm)
        {
            ((Mesh)World.Game["Meshes"][(int)characterPart]).Color = color;
            ((Mesh)World.Game["Meshes"][(int)CharacterPart.LeftHand]).Color = color;
        }

        if (characterPart == CharacterPart.RightArm)
        {
            ((Mesh)World.Game["Meshes"][(int)characterPart]).Color = color;
            ((Mesh)World.Game["Meshes"][(int)CharacterPart.RightHand]).Color = color;
        }

        if (characterPart == CharacterPart.LeftLeg)
        {
            ((Mesh)World.Game["Meshes"][(int)characterPart]).Color = color;
            ((Mesh)World.Game["Meshes"][(int)CharacterPart.LeftFoot]).Color = color;
        }

        if (characterPart == CharacterPart.RightLeg)
        {
            ((Mesh)World.Game["Meshes"][(int)characterPart]).Color = color;
            ((Mesh)World.Game["Meshes"][(int)CharacterPart.RightFoot]).Color = color;
        }        


    }

    public static void LoadHat(string meshUrl, string? texture = null)
    {
        Mesh mesh = new Mesh(meshUrl);

        mesh.Initialize();

        mesh.Position = new Vector3(0, 3.3f, 0);

        World.Game["Meshes"].Add(mesh);
    }

    public static void ResetColor()
    {
        SetColor(CharacterPart.Head, 0xffffff);
        SetColor(CharacterPart.Torso, 0xffffff);
        SetColor(CharacterPart.LeftArm, 0xffffff);
        SetColor(CharacterPart.RightArm, 0xffffff);
        SetColor(CharacterPart.LeftLeg, 0xffffff);
        SetColor(CharacterPart.RightLeg, 0xffffff);
    }
    public static void Render()
    {

        //GL.ClearColor(new Color4(1, 1, 1, 1));
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        if (World.ActiveCamera != null)
        {
            World.ActiveCamera.Update();
            CameraShaderUtil.PassCameraToShader(ref World.ActiveCamera, ref testShader);
        }

        for (int i = 0; i < World.Game["Meshes"].Count; i++)
        {
            Mesh mesh = (Mesh)World.Game["Meshes"][i];

            byte isTextureEnabled = 1;


            if (mesh.Texture == null)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                isTextureEnabled = 0;
            }
            else
            {
                mesh.Texture.UseTexture();
            }

            // could have been used bit flags but too sleepy

            GL.Uniform3(GL.GetUniformLocation(testShader.ShaderProgramId, "objPos"), mesh.Position);
            GL.Uniform1(GL.GetUniformLocation(testShader.ShaderProgramId, "hasNoLighting"), mesh.HasNoLighting == true ? 1 : 1);
            GL.Uniform1(GL.GetUniformLocation(testShader.ShaderProgramId, "color"), (int)mesh.Color);
            GL.Uniform1(GL.GetUniformLocation(testShader.ShaderProgramId, "isTextureEnabled"), isTextureEnabled);
            GL.Uniform1(GL.GetUniformLocation(testShader.ShaderProgramId, "isVertexColorEnabled"), mesh.colors.Count != 0 ? 1 : 0);
            mesh.Draw();
        }


    }

    public static void UnloadTextures()
    {
        GL.DeleteTextures(textures.Count, textures.Skip(_protectedTextureIndex)
                                                  .ToArray()
                                                  .Select(texture => texture.OpenglTextureId)
                                                  .ToArray());
    }

    public static void UnloadMeshes()
    {
        List<object> meshes = World.Game["Meshes"];

        for (int i = _protectedMeshIndex; i < meshes.Count; i++)
        {
            Mesh mesh = (Mesh)meshes[i];

            GL.DeleteBuffers(5, new int[] { mesh.ElementBufferObject, mesh.TextureCoordBufferObject, mesh.NormalBufferObject, mesh.VerticeBufferObject, mesh.VertexColorBufferObject });
            GL.DeleteVertexArrays(1, new int[] { mesh.VertexArrayObject });
        }
    }

    public static byte[] GetImageBytes()
    {
        byte[] data = new byte[400 * 800 * 4];

        GL.ReadPixels(0, 0, 400, 800, PixelFormat.Rgba, PixelType.UnsignedByte, data);

        return data;
    }
}