using DanielSteginkUtils.Helpers.Shields;
using DanielSteginkUtils.Utilities;
using System.Collections;

namespace DivineFury.Charm_Helpers
{
    public class LamentShield : ShieldHelper
    {
        /// <summary>
        /// Godseeker's Lament, for 2 notches worth of Exaltation, gives the player a chance to ignore damage
        /// </summary>
        /// <returns></returns>
        public override bool CustomShieldCheck()
        {
            int random = UnityEngine.Random.Range(1, 101);
            int threshold = (int)(2 * NotchCosts.ShieldChancePerNotch());
            //DivineFury.Instance.Log($"Lament shield: {random} vs {threshold}");

            return random <= threshold;
        }

        /// <summary>
        /// Godseeker's Lament is themed after the Godseeker being consumed by the Void, so we'll flash black
        /// </summary>
        /// <returns></returns>
        public override IEnumerator CustomEffects()
        {
            SpriteFlash flash = ClassIntegrations.GetField<HeroController, SpriteFlash>(HeroController.instance, "spriteFlash");
            flash.flash(UnityEngine.Color.black, 1f, 0.4f, 0.5f, 0.4f);

            return base.CustomEffects();
        }
    }
}
