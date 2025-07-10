public enum RenderOperationType
{
    Fullbody,
    Headshot,
    Item
}
public class RenderOperation
{
    public Guid Id { get; set; }
    public string? Face { get; set; }
    public string? Shirt { get; set; }
    public string? Pants { get; set; }
    public string? Hat { get; set; }
    public uint? HeadColor { get; set; }
    public uint? TorsoColor { get; set; }
    public uint? RightArmColor { get; set; }
    public uint? LeftArmColor { get; set; }
    public uint? RightLegColor { get; set; }
    public uint? LeftLegColor { get; set; }
    public RenderOperationType RenderOperationType { get; set; }
}