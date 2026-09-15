using NotificationPreferences.Client.ViewModels;

namespace NotificationPreferences.Client.Models;

/// <summary>
/// A category header plus its topic rows, in the shape a grouped CollectionView expects.
/// </summary>
public sealed class NotificationCategoryGroup : List<TopicPreferenceViewModel>
{
    public NotificationCategoryGroup(
        int id,
        string code,
        string displayName,
        IEnumerable<TopicPreferenceViewModel> topics)
        : base(topics)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
    }

    public int Id { get; }

    public string Code { get; }

    public string DisplayName { get; }

    public string TopicCountLabel => Count == 1 ? "1 topic" : $"{Count} topics";
}
