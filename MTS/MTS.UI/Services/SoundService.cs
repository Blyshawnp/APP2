using System.Media;
using System.Windows;

namespace MTS.UI.Services;

public class SoundService : ISoundService
{
    public void PlaySuccess() => SystemSounds.Beep.Play();
    public void PlayError()   => SystemSounds.Hand.Play();
    public void PlayWarning() => SystemSounds.Exclamation.Play();
}
