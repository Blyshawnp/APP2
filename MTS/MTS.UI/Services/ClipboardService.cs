using System.Windows;

namespace MTS.UI.Services;

public class ClipboardService : IClipboardService
{
    public void SetText(string text)
        => Clipboard.SetText(text);
}
