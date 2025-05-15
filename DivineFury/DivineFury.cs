using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using Modding;
using MonoMod.RuntimeDetour;
using SFCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DivineFury
{
    public class DivineFury : Mod, IMod, ILocalSettings<LocalSaveData>
    {
        public override string GetVersion() => "1.0.0.0";

        #region Save Data
        public void OnLoadLocal(LocalSaveData s) => SharedData.localSaveData = s;

        public LocalSaveData OnSaveLocal() => SharedData.localSaveData;
        #endregion

        public DivineFury() : base("Divine Fury") { }

        #region Initialization
        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            SharedData.Log("Initializing Mod");

            ApplyHooks();

            InitializeCharm();

            AddCharmToItemChanger();

            SharedData.Log("Mod initialized");
        }

        /// <summary>
        /// Applies mod hooks relating to charm infrastructure
        /// </summary>
        private void ApplyHooks()
        {
            SharedData.Log("Applying hooks");

            ModHooks.LanguageGetHook += GetCharmText;
            ModHooks.GetPlayerBoolHook += GetCharmBools;
            ModHooks.SetPlayerBoolHook += SetCharmBools;
            ModHooks.GetPlayerIntHook += GetCharmCosts;
            On.GameManager.EnterHero += OnEnterHero;

            if (ModHooks.GetMod("DebugMod") != null)
            {
                AddToGiveAllCharms(GiveCharm);
            }

            SharedData.Log("Hooks applied");
        }

        /// <summary>
        /// Initializes the charm and its sprite
        /// </summary>
        private void InitializeCharm()
        {
            SharedData.Log("Initializing charm");

            // Add the charm to the charm list and get its new ID number
            SharedData.divineFury.Sprite = SpriteHelper.Get(SharedData.divineFury.InternalName());
            SharedData.divineFury.Num = CharmHelper.AddSprites(new Sprite[] { SharedData.divineFury.Sprite })[0];
            SharedData.Log($"Sprite found for {SharedData.divineFury.Name}. ID assigned: {SharedData.divineFury.Num}");

            // Apply charm effects
            SharedData.divineFury.ApplyEffects();

            SharedData.Log("Charm initialized");
        }

        /// <summary>
        /// Adds charm to Item Changer so it can be placed on the map
        /// </summary>
        private void AddCharmToItemChanger()
        {
            SharedData.Log("Adding charm to Item Changer");

            // Tag the item for ConnectionMetadataInjector, so that MapModS and other mods recognize the items we're adding as charms.
            var item = new ItemChanger.Items.CharmItem()
            {
                charmNum = SharedData.divineFury.Num,
                name = SharedData.divineFury.InternalName(),
                UIDef = new MsgUIDef()
                {
                    name = new LanguageString("UI", $"CHARM_NAME_{SharedData.divineFury.Num}"),
                    shopDesc = new LanguageString("UI", $"CHARM_DESC_{SharedData.divineFury.Num}"),
                    sprite = new ItemChangerSprite(SharedData.divineFury.InternalName(), SharedData.divineFury.Sprite)
                }
            };

            var mapModTag = item.AddTag<InteropTag>();
            mapModTag.Message = "RandoSupplementalMetadata";
            mapModTag.Properties["ModSource"] = GetName();
            mapModTag.Properties["PoolGroup"] = "Charms";

            // Add the charm and its location to ItemChanger for placement
            Finder.DefineCustomItem(item);
            Finder.DefineCustomLocation(SharedData.divineFury.Location());

            SharedData.Log("Charm added to Item Changer");
        }
        #endregion

        #region Charm Data and Settings
        /// <summary>
        /// Gets text data related to the charms (name and description)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sheetName"></param>
        /// <param name="orig"></param>
        /// <returns></returns>
        private string GetCharmText(string key, string sheetName, string orig)
        {
            if (key.Equals($"CHARM_NAME_{SharedData.divineFury.Num}"))
            {
                return SharedData.divineFury.Name;
            }
            else if (key.Equals($"CHARM_DESC_{SharedData.divineFury.Num}"))
            {
                return SharedData.divineFury.Description;
            }

            return orig;
        }

        /// <summary>
        /// Gets boolean values related to the charms (equipped, new, found)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private bool GetCharmBools(string key, bool defaultValue)
        {
            if (key.Equals($"gotCharm_{SharedData.divineFury.Num}"))
            {
                return SharedData.localSaveData.charmFound;
            }
            else if (key.Equals($"equippedCharm_{SharedData.divineFury.Num}"))
            {
                return SharedData.localSaveData.charmEquipped;
            }
            else if (key.Equals($"newCharm_{SharedData.divineFury.Num}"))
            {
                return SharedData.localSaveData.charmNew;
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets boolean values related to the charms (equipped, new, found)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="orig"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool SetCharmBools(string key, bool orig)
        {
            if (key.Equals($"gotCharm_{SharedData.divineFury.Num}"))
            {
                SharedData.localSaveData.charmFound = orig;
            }
            else if (key.Equals($"equippedCharm_{SharedData.divineFury.Num}"))
            {
                SharedData.localSaveData.charmEquipped = orig;
            }
            else if (key.Equals($"newCharm_{SharedData.divineFury.Num}"))
            {
                SharedData.localSaveData.charmNew = orig;
            }

            return orig;
        }

        /// <summary>
        /// Gets the costs of charms
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int GetCharmCosts(string key, int defaultValue)
        {
            if (key.Equals($"charmCost_{SharedData.divineFury.Num}"))
            {
                return SharedData.localSaveData.charmCost;
            }

            return defaultValue;
        }
        #endregion

        #region Charm Placement
        /// <summary>
        /// Triggers when the player enters a new area
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="additiveGateSearch"></param>
        private void OnEnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch)
        {
            //SharedData.Log($"Scene entered: {self.sceneName}");
            PlaceCharms();

            orig(self, additiveGateSearch);
        }

        /// <summary>
        /// Gets charms from ItemChanger and places them on the map
        /// </summary>
        public void PlaceCharms()
        {
            SharedData.Log("Placing charm");

            ItemChangerMod.CreateSettingsProfile(false, false);
            List<AbstractPlacement> placements = new List<AbstractPlacement>();

            // Only place charms player hasn't collected
            if (!SharedData.localSaveData.charmFound)
            {
                AbstractLocation location = Finder.GetLocation(SharedData.divineFury.InternalName());
                AbstractPlacement placement = location.Wrap();
                AbstractItem item = Finder.GetItem(SharedData.divineFury.InternalName());
                placement.Add(item);
                placements.Add(placement);

                ItemChangerMod.AddPlacements(placements, PlacementConflictResolution.Replace);
                SharedData.Log("Charm placed");
            }
        }
        #endregion

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
            SharedData.localSaveData.charmFound = true;
        }
        #endregion
    }
}