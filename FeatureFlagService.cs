
public interface IFeatureFlagService
{
    List<Flag> GetAllFeatureFlags();
    FeatureFlagging UseFeatureFlag(string flagId, string upn);
    List<FeatureFlagging> GetUsersFeatureFlags(string upn);
}

public class FeatureFlagService : IFeatureFlagService
{
    private readonly List<Flag> _featureFlags;
    private readonly List<FeatureFlagUserGroup> _featureFlagUserGroups;

    public FeatureFlagService(IOptions<List<Flag>> featureFlags, IOptions<List<FeatureFlagUserGroup>> featureFlagUserGroups)
    {
        _featureFlags = featureFlags.Value;
        _featureFlagUserGroups = featureFlagUserGroups.Value;
    }

    public List<Flag> GetAllFeatureFlags()
    {
        return _featureFlags;
    }

    public FeatureFlagging UseFeatureFlag(string flagId, string upn = "")
    {
        var isEnabled = IsFeatureFlagEnabled(flagId, upn);
        return new FeatureFlagging(flagId, isEnabled);
    }

    public List<FeatureFlagging> GetUsersFeatureFlags(string upn)
    {
        var flags = new List<FeatureFlagging>();

        bool UserIsIn(FeatureFlagFilter filter) =>
            filter?.Users?.Count > 0 && filter.Users.Contains(upn);

        bool HasNoUsers(FeatureFlagFilter filter) =>
            filter?.Users?.Count == 0;

        bool HasNoUserGroups(FeatureFlagFilter filter) =>
            filter?.UserGroups?.Count == 0;

        bool UserIsInUserGroup(string groupId) =>
            _featureFlagUserGroups.FirstOrDefault(ug => ug.Id.Equals(groupId, StringComparison.InvariantCultureIgnoreCase)) != null &&
            _featureFlagUserGroups.First(ug => ug.Id.Equals(groupId, StringComparison.InvariantCultureIgnoreCase)).Users.Contains(upn);

        bool UserIsInGroup(FeatureFlagFilter filter) =>
            filter?.UserGroups?.Count > 0 && filter.UserGroups.Any(g => UserIsInUserGroup(g));

        bool IsStartValid(FeatureFlagFilter filter) =>
            filter?.Timeline?.Start == null || DateTime.Now >= filter.Timeline.Start;

        bool IsEndValid(FeatureFlagFilter filter) =>
            filter?.Timeline?.End == null || DateTime.Now <= filter.Timeline.End;

        foreach (var f in _featureFlags)
        {
            var filter = f.Filter;

            if (filter == null)
            {
                flags.Add(new FeatureFlagging(f.Id, true));
                continue;
            }

            if (IsStartValid(filter) && IsEndValid(filter) && 
                (UserIsIn(filter) || HasNoUsers(filter) || UserIsInGroup(filter) || HasNoUserGroups(filter) ))
            {
                flags.Add(new FeatureFlagging(f.Id, true));
                continue;
            }

            if (UserIsIn(filter))
            {
                flags.Add(new FeatureFlagging(f.Id, true));
            }
        }

        return flags;
    }

    private bool IsFeatureFlagEnabled(string flagId, string upn = "", int dependencyCount = 0)
    {
        if (!_featureFlags.Exists(flag => flag.Id == flagId))
            return false;

        var flag = _featureFlags.Find(flag => flag.Id == flagId);

        if (flag.Filter == null)
            return true;

        if (flag.Filter.Features != null && dependencyCount == 0)
        {
            foreach (var featureId in flag.Filter.Features)
            {
                if (!IsFeatureFlagEnabled(featureId, upn, dependencyCount + 1))
                    return false;
            }
        }

        if (flag.Filter.Timeline != null)
        {
            if (flag.Filter.Timeline.Start != null && flag.Filter.Timeline.Start > DateTime.Now)
                return false;

            if (flag.Filter.Timeline.End != null && flag.Filter.Timeline.End < DateTime.Now)
                return false;
        }

        if (flag.Filter.Users != null && upn != "")
        {
            if (!flag.Filter.Users.Exists(email => email == upn))
                return false;
        }

        if (flag.Filter.UserGroups != null && upn != "")
        {
            foreach(var userGroupId in flag.Filter.UserGroups)
            {
                var userGroup = _featureFlagUserGroups.FirstOrDefault(ug => ug.Id.Equals(userGroupId, StringComparison.InvariantCultureIgnoreCase));
                return userGroup != null && userGroup.Users.Contains(upn);
            }
        }

        return true;
    }
}