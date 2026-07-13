using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Squall.SquallCode.Relics;


public interface IFirepowerRelic
{
    void GainFirepowerProgress(int amount = 1);
    Task ConsumeFirepower(PlayerChoiceContext? choiceContext);

    int GetFirepowerProgressForUI();
    bool IsFirepowerChargedForUI();
}
