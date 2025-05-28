
public class FeatureFlagFilter
{
    public List<string>? Features { get; set; }
    public List<string>? Users { get; set; }
    public List<string>? UserGroups { get; set; }
    public FeatureFlagTimeline? Timeline { get; set; }
}

public class FeatureFlagTimeline
{
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
}

public class FeatureFlagUserGroup
{
    public string Id { get; set; }
    public List<string> Users { get; set; }
}