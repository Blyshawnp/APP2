namespace MTS.Core.Models.Settings;

public class DonorList
{
    public List<Donor> NewDonors { get; set; } = new();
    public List<Donor> ExistingMembers { get; set; } = new();
    public List<Donor> IncreaseSustaining { get; set; } = new();
}
