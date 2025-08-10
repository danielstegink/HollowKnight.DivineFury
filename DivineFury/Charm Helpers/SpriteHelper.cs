using UnityEngine;

namespace DivineFury
{
    public static class SpriteHelper
    {
        /// <summary>
        /// Gets the charm's sprite (icon) from the mod's embedded resources
        /// </summary>
        /// <returns></returns>
        public static Sprite Get(string spriteFileName)
        {
            return DanielSteginkUtils.Helpers.SpriteHelper.GetLocalSprite($"DivineFury.Resources.{spriteFileName}.png", "DivineFury");
        }
    }
}