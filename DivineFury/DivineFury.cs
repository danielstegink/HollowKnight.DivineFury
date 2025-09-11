using DivineFury.Helpers;
using Modding;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DivineFury
{
    public class DivineFury : Mod, IMod, ILocalSettings<LocalSaveData>
    {
        public static DivineFury Instance;

        public override string GetVersion() => "1.2.1.1";

        #region Save Data
        public void OnLoadLocal(LocalSaveData s)
        {
            SharedData.localSaveData = s;

            if (SharedData.divineFury != null)
            {
                SharedData.divineFury.OnLoadLocal();
            }
        }

        public LocalSaveData OnSaveLocal()
        {
            if (SharedData.divineFury != null)
            {
                SharedData.divineFury.OnSaveLocal();
            }

            return SharedData.localSaveData;
        }
        #endregion

        public DivineFury() : base("Divine Fury") { }

        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing Mod");

            Instance = new DivineFury();

            if (ModHooks.GetMod("DebugMod") != null)
            {
                AddToGiveAllCharms(GiveCharm);
            }

            SharedData.divineFury = new DivineFuryCharm();

            Log("Mod initialized");
        }

        #region Debug Mod
        /// <summary>
        /// Links the given method into the Debug mod's "GiveAllCharms" function
        /// </summary>
        /// <param name="a"></param>
        public static void AddToGiveAllCharms(Action function)
        {
            var commands = Type.GetType("DebugMod.BindableFunctions, DebugMod");
            if (commands == null)
            {
                return;
            }

            var method = commands.GetMethod("GiveAllCharms", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return;
            }

            new Hook(method, (Action orig) =>
            {
                orig();
                function();
            }
            );
        }

        /// <summary>
        /// Adds the mod's charm to the player (used by Debug Mode mod)
        /// </summary>
        private void GiveCharm()
        {
            SharedData.divineFury.GiveCharm();
        }
        #endregion
    }
}