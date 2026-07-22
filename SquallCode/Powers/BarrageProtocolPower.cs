using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Squall.SquallCode.Powers;

/// <summary>
/// Whenever Firepower is consumed, deal Amount damage to ALL enemies.
/// Triggered by FirepowerRelicBase.ConsumeFirepower.
/// </summary>
public class BarrageProtocolPower : SquallPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnFirepowerConsumed(PlayerChoiceContext? choiceContext)
    {
        Flash();

        //Snapshot the list; enemies may die mid-loop.
        var enemies = base.CombatState.HittableEnemies.ToList();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive)
                continue;

            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                base.Amount,
                ValueProp.Unpowered,
                null,
                null);
        }
    }
}
