namespace SortSchedule.Application.Options;

public sealed class TabuSearchOptions
{
    public int TabuTenure { get; set; } = 50;

    public int MaxIterations { get; set; } = 5000;

    public int NeighborhoodSize { get; set; } = 300;

    public int? RandomSeed { get; set; }
}
