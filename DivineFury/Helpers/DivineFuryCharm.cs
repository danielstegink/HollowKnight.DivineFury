using DanielSteginkUtils.Helpers.Charms.Templates;
using ItemChanger;
using ItemChanger.Locations;
using SFCore.Utils;
using UnityEngine;

namespace DivineFury.Helpers
{
    /// <summary>
    /// Divine Fury sets the player's health to 1
    /// </summary>
    public class DivineFuryCharm : ExaltedCharm
    {
        public DivineFuryCharm() : base(DivineFury.Instance.Name, true) 
        {
            On.HeroController.CharmUpdate += OnUpdate;
        }

        #region Properties
        public override string name => "Divine Fury";

        public override string exaltedName => "Godseeker's Lament";

        public override string description => "Contains the Godseeker's hatred for trespassers.\n\n" +
                                                "Reduces the bearer's health to 1.";

        public override string exaltedDescription => "Contains the Godseeker's fear and desperation.\n\n" +
                                                        "Reduces the bearer's health to 1, but gives a small chance to ignore damage.";

        public override Sprite exaltedIcon => SpriteHelper.Get("GodseekersLament");

        public override int cost => 0;

        public override int exaltedCost => 0;
        #endregion

        protected override Sprite GetSpriteInternal()
        {
            return SpriteHelper.Get("DivineFury");
        }

        public override AbstractLocation ItemChangerLocation()
        {
            // This charm should be placed at the entrance to the Hall of the Gods
            return new CoordinateLocation()
            {
                name = GetItemChangerId(),
                sceneName = "GG_Workshop",
                x = 17.65f,
                y = 6.41f,
                elevation = 0f
            };
        }

        #region Exaltation
        public override bool CanUpgrade()
        {
            return PlayerData.instance.statueStateHollowKnight.completedTier2 ||
                    PlayerData.instance.bossDoorStateTier5.completed;
        }

        public override void Upgrade()
        {
            base.Upgrade();

            if (IsEquipped)
            {
                ResetHelper();
                helper.Start();
            }
        }
        #endregion

        #region Settings
        public override void OnLoadLocal()
        {
            ExaltedCharmState charmSettings = new ExaltedCharmState()
            {
                IsEquipped = SharedData.localSaveData.charmEquipped,
                GotCharm = SharedData.localSaveData.charmFound,
                IsNew = SharedData.localSaveData.charmNew,
                IsUpgraded = SharedData.localSaveData.charmUpgraded
            };

            RestoreCharmState(charmSettings);
        }

        public override void OnSaveLocal()
        {
            ExaltedCharmState charmSettings = GetCharmState();
            SharedData.localSaveData.charmEquipped = IsEquipped;
            SharedData.localSaveData.charmFound = GotCharm;
            SharedData.localSaveData.charmNew = IsNew;
            SharedData.localSaveData.charmUpgraded = IsUpgraded;
        }
        #endregion

        #region Activation
        /// <summary>
        /// When Divine Fury is equipped, set HP to 1 and set Fury of the Fallen to trigger "automatically"
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        private void OnUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
        {
            // Perform charm updates first so that the health charms perform their effects
            orig(self);

            // Get Fury of the Fallen
            GameObject furyOfTheFallen = GameObject.Find("Charm Effects");
            if (IsEquipped)
            {
                // Set health to 1 and blue health to 0
                PlayerData.instance.joniHealthBlue = 0;
                PlayerData.instance.healthBlue = 0;
                PlayerData.instance.health = 1;
                PlayerData.instance.maxHealth = 1;

                SetFury(furyOfTheFallen, PlayerData.instance.equippedCharm_6);
            }
            else
            {
                SetFury(furyOfTheFallen, false);
            }
        }

        /// <summary>
        /// Sets Fury of the Fallen to trigger automatically if Fury of the Fallen is equipped
        /// </summary>
        /// <param name="furyOfTheFallen"></param>
        private void SetFury(GameObject furyOfTheFallen, bool turnFuryOn)
        {
            if (furyOfTheFallen != null)
            {
                // Get FotF's FSM
                PlayMakerFSM fsm = furyOfTheFallen.LocateMyFSM("Fury");

                if (turnFuryOn)
                {
                    // Set FotF to trigger automatically
                    fsm.ChangeTransition("Activate", "HERO HEALED FULL", "Stay Furied");
                    fsm.ChangeTransition("Stay Furied", "HERO HEALED FULL", "Activate");
                    fsm.Fsm.SendEventToFsmOnGameObject(furyOfTheFallen, "Fury", "HERO DAMAGED");
                }
                else
                {
                    // Reset FotF to trigger normally
                    fsm.ChangeTransition("Activate", "HERO HEALED FULL", "Deactivate");
                    fsm.ChangeTransition("Stay Furied", "HERO HEALED FULL", "Deactivate");
                    fsm.Fsm.SendEventToFsmOnGameObject(furyOfTheFallen, "Fury", "HERO HEALED FULL");
                }
            }
        }

        /// <summary>
        /// Activates the charm effects
        /// </summary>
        public override void Equip()
        {
            if (IsUpgraded)
            {
                ResetHelper();
                helper.Start();
            }
        }

        /// <summary>
        /// Deactivates the charm effects
        /// </summary>
        public override void Unequip()
        {
            ResetHelper();
        }

        /// <summary>
        /// Shield helper for Godseeker's Lament
        /// </summary>
        private LamentShield helper;

        /// <summary>
        /// Resets the AllPetsHelper
        /// </summary>
        private void ResetHelper()
        {
            if (helper != null)
            {
                helper.Stop();
            }

            helper = new LamentShield();
        }
        #endregion
    }
}