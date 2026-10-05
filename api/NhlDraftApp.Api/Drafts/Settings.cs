namespace NhlDraftApp.Api.Drafts;

public record Settings(int Rounds = 20, decimal Cap = 104m, int DefenseMin = 2, int Goalies = 2, int Teams = 2)
{
    public string? Problem()
    {
        if (Rounds <= 0 || Cap <= 0)
            return "Rounds and cap must be greater than zero.";
        if (DefenseMin < 0 || Goalies < 0 || Teams < 0)
            return "Defensemen, goalies and teams cannot be negative.";
        if (DefenseMin + Goalies + Teams > Rounds)
            return "Defensemen, goalies and teams required exceed the number of rounds.";
        return null;
    }
}
