using BaseLib.Abstracts;
using Squall.SquallCode.Extensions;
using Godot;

namespace Squall.SquallCode.Character;

public class SquallPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Squall.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}