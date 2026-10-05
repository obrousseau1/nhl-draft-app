namespace NhlDraftApp.Api.Drafts;

public record Settings(int Rounds = 20, decimal Cap = 104m, int DefenseMin = 2, int Goalies = 2, int Teams = 2, int MaxPoolers = 12)
{
    public const int MaxRounds = 50;
    public const decimal MaxCap = 1000m;
    public const int PoolerLimit = 30;

    public string? Problem()
    {
        if (Rounds is <= 0 or > MaxRounds)
            return $"Rounds must be between 1 and {MaxRounds}.";
        if (Cap is <= 0 or > MaxCap)
            return $"Cap must be greater than 0 and at most {MaxCap} M$.";
        if (!IsCount(DefenseMin) || !IsCount(Goalies) || !IsCount(Teams))
            return "Defensemen, goalies and teams must each be between 0 and the number of rounds.";
        if (MaxPoolers is < 2 or > PoolerLimit)
            return $"Maximum poolers must be between 2 and {PoolerLimit}.";
        if (DefenseMin + Goalies + Teams > Rounds)
            return "Defensemen, goalies and teams required exceed the number of rounds.";
        return null;
    }

    private bool IsCount(int value) => value >= 0 && value <= Rounds;
}
