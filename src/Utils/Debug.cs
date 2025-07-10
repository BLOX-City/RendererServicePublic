using OpenTK.Graphics.OpenGL;

namespace BLOXCityRenderer;
public static class Debug {
    public static void GLError() {
        ErrorCode errorCode = GL.GetError();
        if(errorCode != ErrorCode.NoError) {
            Console.WriteLine(errorCode.ToString());
        }            
    }
}