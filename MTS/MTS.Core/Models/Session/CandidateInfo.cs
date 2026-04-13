using MTS.Core.Enums;

namespace MTS.Core.Models.Session;

public class CandidateInfo
{
    public string CandidateName { get; set; } = string.Empty;
    public Pronoun Pronoun { get; set; } = Pronoun.They;
    public bool IsFinalAttempt { get; set; }
}
