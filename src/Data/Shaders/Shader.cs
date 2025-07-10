using OpenTK.Graphics.OpenGL;

namespace BLOXCityRenderer.Data;
public class Shader {
    public ShaderType ShaderType {get; set;}
    public string Source {get; set;} = String.Empty;
    public int ShaderId {get; set;}
    public void LoadFromFile(string path) {
        Source = File.ReadAllText(path);
    }

    public void Compile() {
        ShaderId = GL.CreateShader(ShaderType);
        GL.ShaderSource(ShaderId, Source);
        GL.CompileShader(ShaderId);
    }
    public Shader() {}
    public Shader(string path) {
        this.LoadFromFile(path);
    }
}

public class ShaderProgram {
    public Shader? Vertex {get; set;}
    public Shader? Fragment {get; set;}

    public int ShaderProgramId {get; set;}

    public ShaderProgram() {}

    public void Compile(bool cleanup = true) {
        ShaderProgramId = GL.CreateProgram();
        if(Vertex != null)
            GL.AttachShader(ShaderProgramId, Vertex.ShaderId);
        if(Fragment != null)
            GL.AttachShader(ShaderProgramId, Fragment.ShaderId);
        GL.LinkProgram(ShaderProgramId);

        // Cleanup
        if(cleanup) {
            if(Vertex != null) {
                GL.DeleteShader(Vertex.ShaderId);
                Vertex = null;
            }
            if(Fragment != null) {
                GL.DeleteShader(Fragment.ShaderId);
                Fragment = null;
            }
        }
    }

    public void Use() {
        GL.UseProgram(ShaderProgramId);
    }
}