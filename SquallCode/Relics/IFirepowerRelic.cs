namespace Squall.SquallCode.Relics;


public interface IFirepowerRelic
{
    void GainFirepowerProgress(int amount = 1);
    void ConsumeFirepower();

    int GetFirepowerProgressForUI();
    bool IsFirepowerChargedForUI();
}
