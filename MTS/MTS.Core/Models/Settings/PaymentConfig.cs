namespace MTS.Core.Models.Settings;

public class PaymentConfig
{
    public string CcType { get; set; } = string.Empty;
    public string CcNumber { get; set; } = string.Empty;
    public string CcExpiration { get; set; } = string.Empty;
    public string CcCvv { get; set; } = string.Empty;
    public string EftRouting { get; set; } = string.Empty;
    public string EftAccount { get; set; } = string.Empty;
}
