using System;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace MdbListRatingsProvider;

public sealed class Plugin : BasePlugin<Configuration>, IHasWebPages, IHasThumbImage
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public static Plugin Instance { get; private set; } = null!;

    public override Guid Id => new("2f0cbf4d-249d-4f22-b451-2f2e1766fb7e");

    public override string Name => "MDBList Ratings Provider";

    public override string Description => "Fetches Rotten Tomatoes critic and audience scores from MDBList during metadata refresh.";

    public override string ConfigurationFileName => "MdbListRatingsProvider.xml";

    public Stream GetThumbImage()
    {
        var type = GetType();
        return type.Assembly.GetManifestResourceStream(type.Namespace + ".Images.thumb.png")
               ?? type.Assembly.GetManifestResourceStream("MdbListRatingsProvider.Images.thumb.png")
               ?? throw new FileNotFoundException("Could not find thumb.png as an embedded resource.");
    }

    public ImageFormat ThumbImageFormat => ImageFormat.Png;

    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = "MdbListRatingsProviderConfig",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.html"
            },
            new PluginPageInfo
            {
                Name = "MdbListRatingsProviderConfigJs",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.js"
            }
        };
    }
}
