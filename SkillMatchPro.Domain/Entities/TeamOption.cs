using SkillMatchPro.Domain.ValueObjects;

namespace SkillMatchPro.Domain.Entities;

public class TeamOption
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string OptionName { get; private set; }
    public decimal TotalCostPerWeek { get; private set; }
    public decimal QualityScore { get; private set; }
    public decimal RiskScore { get; private set; }
    public bool MeetsAllRequirements { get; private set; }

    private readonly List<HourAllocation> _allocations = new();
    public IReadOnlyCollection<HourAllocation> Allocations => _allocations.AsReadOnly();

    private readonly Dictionary<string, string> _tradeOffs = new();
    public IReadOnlyDictionary<string, string> TradeOffs => _tradeOffs;

    private TeamOption()
    {
        OptionName = string.Empty;
    }

    public TeamOption(Guid projectId, string optionName)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        OptionName = optionName;
        QualityScore = 0;
        RiskScore = 0;
        TotalCostPerWeek = 0;
        MeetsAllRequirements = false;
    }

    public void AddAllocation(HourAllocation allocation, decimal hourlyCost)
    {
        _allocations.Add(allocation);
        TotalCostPerWeek += allocation.HoursPerWeek * hourlyCost;
    }

    public void SetScores(decimal qualityScore, decimal riskScore, bool meetsRequirements)
    {
        QualityScore = qualityScore;
        RiskScore = riskScore;
        MeetsAllRequirements = meetsRequirements;
    }

    public void AddTradeOff(string aspect, string description)
    {
        _tradeOffs[aspect] = description;
    }

    public int GetTotalHoursPerWeek()
    {
        return _allocations.Sum(a => a.HoursPerWeek);
    }

    public decimal GetEffectiveHoursPerWeek()
    {
        return _allocations.Sum(a => a.GetEffectiveHours());
    }
}