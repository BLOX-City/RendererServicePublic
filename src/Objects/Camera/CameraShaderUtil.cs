using BLOXCityRenderer.Data;
using BLOXCityRenderer.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BLOXCityRenderer.Utils;
public static class CameraShaderUtil {
    public static void PassCameraToShader(ref Camera camera, ref ShaderProgram shader) {
        Matrix4 projection = camera.Projection;
        Matrix4 view = camera.View;
        GL.UniformMatrix4(GL.GetUniformLocation(shader.ShaderProgramId, "projection"), false, ref projection);
        GL.UniformMatrix4(GL.GetUniformLocation(shader.ShaderProgramId, "view"), false, ref view);

    }
}