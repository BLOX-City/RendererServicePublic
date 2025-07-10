using BLOXCityRenderer.Data;
using OpenTK.Mathematics;

namespace BLOXCityRenderer.Objects;

public class Camera {
    public Vector3 Position {get; set;} = Vector3.Zero;
    public float Yaw {get; set;} = 0.0f;
    public float Pitch {get; set;} = 0.0f;
    public float Roll {get; set;} = 0.0f;
    public Vector3 Rotation {get; set;} = Vector3.Zero;
    public Matrix4 Projection {get; set;} = Matrix4.Identity;
    public Matrix4 View {get; set;} = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
    public float Fov {get; set;} = 80.0f;
    public Vector2 ScreenSize {get; set;}

    public int CorrespondingId {get; set;}

    public void GenerateProjection() {
        Projection = Matrix4.CreatePerspectiveFieldOfView(Fov * MathF.PI / 180, ScreenSize.X / ScreenSize.Y, 0.01f, 10000.0f);
        View *= Matrix4.CreateTranslation(0, 0, -10);
    }

    public void Move(Vector3 direction) {
        Vector3 p = new Vector3(
            MathF.Sin(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch)),
             MathF.Sin(MathHelper.DegreesToRadians(Pitch)), 
            MathF.Cos(MathHelper.DegreesToRadians(Yaw)) * MathF.Cos(MathHelper.DegreesToRadians(Pitch))) * direction;
        Position += p;
    }

    public void Update() {
        View = Matrix4.CreateTranslation(Position) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X));
    }

    public Camera() {

    }

    public Camera(Vector2 screenSize) {
        this.ScreenSize = screenSize;
    }

    public static implicit operator GameObject(Camera camera) => new GameObject(typeof(Camera), camera);
}