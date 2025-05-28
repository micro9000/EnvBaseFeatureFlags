public class Flag
{
    public string Id { get; set; }
    public string Description { get; set; }
    public List<string> Modules { get; set; }
    public List<string> Owners { get; set; }
    public FeatureFlagFilter? Filter { get; set; }
}
