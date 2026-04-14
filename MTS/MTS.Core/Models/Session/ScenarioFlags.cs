namespace MTS.Core.Models.Session;

public class ScenarioFlags
{
    public bool HasPhone { get; set; }
    public bool HasSms { get; set; }
    public bool HasEnews { get; set; }
    public bool HasShipping { get; set; }
    public bool HasCcFee { get; set; }

    public static ScenarioFlags GenerateRandom()
    {
        var rng = Random.Shared;
        return new ScenarioFlags
        {
            HasPhone    = rng.NextDouble() > 0.5,
            HasSms      = rng.NextDouble() > 0.5,
            HasEnews    = rng.NextDouble() > 0.5,
            HasShipping = rng.NextDouble() > 0.5,
            HasCcFee    = rng.NextDouble() > 0.7
        };
    }
}
