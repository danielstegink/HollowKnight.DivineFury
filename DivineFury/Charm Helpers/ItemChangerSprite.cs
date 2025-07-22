using ItemChanger;
using Newtonsoft.Json;
using UnityEngine;

namespace DivineFury
{
    /// <summary>
    /// Image (sprite) of the charm. Used by ItemChanger when placing the
    /// charm on the map
    /// </summary>
    internal class ItemChangerSprite : ISprite
    {
        /// <summary>
        /// Sprite's name (should match the charm icon's file name)
        /// </summary>
        public string name;

        [JsonIgnore] 
        public Sprite Value => _value;
        private Sprite _value;

        public ItemChangerSprite(string name, Sprite sprite)
        {
            this.name = name;
            _value = sprite;
        }

        public ISprite Clone()
        {
            return (ISprite)base.MemberwiseClone();
        }
    }
}