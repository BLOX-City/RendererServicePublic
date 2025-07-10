using BLOXCityRenderer.Objects;

namespace BLOXCityRenderer.Objects;

public class World {
    public Dictionary<string, List<object>> Game {get; set;} = new Dictionary<string, List<object>>() {
        {"Camera", new List<object>() },
        {"Meshes", new List<object>()}
    };

    public BLOXCityRenderer.Objects.Camera? ActiveCamera;


    public void MakeActiveCamera(int index) {
        ActiveCamera = (Camera)Game["Camera"][index];
        ActiveCamera.CorrespondingId = index;
    }

    public int AddCamera(Camera camera) {
        this.Game["Camera"].Add(camera);
        return this.Game["Camera"].Count - 1;
    }
}