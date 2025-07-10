namespace BLOXCityRenderer.Data;


public class GameObject {
    public object? Data {get; set;}
    public Type? Type {get; set;}
    public GameObject() {}
    public GameObject(Type? type, object data) {
        this.Data = data;
        this.Type = type;
    }
}