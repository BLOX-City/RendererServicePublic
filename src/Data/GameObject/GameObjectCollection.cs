namespace BLOXCityRenderer.Data;

public class GameObjectCollection {
    public enum GameObjectCollectionType {
        Objects,
        Meshes,
        Textures,
        Cameras,

    }
    public List<GameObject> GameObjects {get; set;} = new List<GameObject>();
    public GameObjectCollectionType Type {get; set;}

}



