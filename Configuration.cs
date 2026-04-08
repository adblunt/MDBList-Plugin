using MediaBrowser.Model.Plugins;

namespace MdbListRatingsProvider;

public sealed class Configuration : BasePluginConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
}
