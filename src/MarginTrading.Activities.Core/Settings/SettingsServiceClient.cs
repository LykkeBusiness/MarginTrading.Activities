using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.Activities.Core.Settings
{
    [UsedImplicitly]
    public class SettingsServiceClient
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}