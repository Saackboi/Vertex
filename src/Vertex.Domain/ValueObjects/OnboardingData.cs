namespace Vertex.Domain.ValueObjects;

public class OnboardingData
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = new();
    public List<WorkEntry> Experiences { get; set; } = new();
    public List<EducationEntry> Educations { get; set; } = new();
}

public class WorkEntry
{
    public string Company { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateRange DateRange { get; set; } = new();
}

public class EducationEntry
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public DateRange DateRange { get; set; } = new();
}

public class DateRange
{
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
}
