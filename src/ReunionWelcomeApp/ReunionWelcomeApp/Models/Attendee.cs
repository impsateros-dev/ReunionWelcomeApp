using System;

namespace ReunionWelcomeApp.Models;

public class Attendee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; } = DateTime.Now;
}