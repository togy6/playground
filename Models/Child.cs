namespace PlaygroundDashboard.Models;

public class Child
{
    public int      Id        { get; set; }
    public string   Name      { get; set; } = "";
    public DateTime StartedAt { get; set; }
    public int      Duration  { get; set; }  // minutes
    public string   GuardianName  { get; set; } = "";
    public string   GuardianPhone { get; set; } = "";
    public bool     IsActive  { get; set; } = true;
}
