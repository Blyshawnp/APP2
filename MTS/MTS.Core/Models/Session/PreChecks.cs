namespace MTS.Core.Models.Session;

public class PreChecks
{
    public bool? HeadsetUsb { get; set; }
    public bool? NoiseCancelling { get; set; }
    public string HeadsetBrand { get; set; } = string.Empty;
    public bool? VpnPresent { get; set; }
    public bool? VpnCanDisable { get; set; }
    public bool? ChromeIsDefault { get; set; }
    public bool? ExtensionsDisabled { get; set; }
    public bool? PopupsAllowed { get; set; }
}
