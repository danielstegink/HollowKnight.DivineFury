using Satchel;
using System.IO;
using System.Reflection;
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
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] manifestedResourceNames = assembly.GetManifestResourceNames();
            SharedData.Log(string.Join(",", manifestedResourceNames));

            using (Stream stream = assembly.GetManifestResourceStream($"DivineFury.Resources.{spriteFileName}.png"))
            {
                // Convert stream to bytes
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                // Create texture from bytes
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(bytes, true);

                // Create sprite from texture
                return Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f));
            }
        }
    }
}