using BaseLib.Abstracts;
using BaseLib.Utils;
using Squall.SquallCode.Character;

namespace Squall.SquallCode.Potions;

[Pool(typeof(SquallPotionPool))]
public abstract class SquallPotion : CustomPotionModel;