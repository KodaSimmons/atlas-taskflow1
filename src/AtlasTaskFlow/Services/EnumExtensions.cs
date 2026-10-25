using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AtlasTaskFlow.Services;

public static class EnumExtensions
{
    /// <summary>Returns the [Display(Name)] text for an enum value, e.g. "Needs Review".</summary>
    public static string DisplayName(this Enum value) =>
        value.GetType().GetMember(value.ToString()).FirstOrDefault()?.GetCustomAttribute<DisplayAttribute>()?.GetName()
        ?? value.ToString();
}
