using MiraModule.Utilities;

namespace MiraModule.Modifiers.ChaosTokens.Effects;

public sealed class TokenTasks : TokenEffect
{
    public override ChaosEffects Effect => ChaosEffects.Tasks;
    public override string ModifierName => "Token Tasks";
    public override string Notification => "You received fake tasks!";
    public override bool Negative => true;
    public override bool RemoveAfterMeeting => false;

    public override void OnActivate()
    {
        base.OnActivate();
        if (!Player.AmOwner) return;

        var tasks = ChaosTokensUtils.GetUncompletedTasks(Player);
        foreach (var task in tasks)
        {
            task.Complete();
        }
    }
}
