using ItemChanger.Locations;
using ItemChanger;
using UnityEngine;
using SFCore.Utils;

namespace DivineFury.Charms
{
    /// <summary>
    /// Divine Fury sets the player's health to 1
    /// </summary>
    public class DivineFuryCharm : Charm
    {
        public override string Name => "Divine Fury";

        public override string Description => "Manifestation of the Godseeker's outrage and hatred for trespassers.\n\n" +
                                                "Reduces the bearer's health to 1.";

        public override AbstractLocation Location()
        {
            // This charm should be placed at the entrance to the Hall of the Gods
            return new CoordinateLocation()
            {
                name = InternalName(),
                sceneName = "GG_Workshop",
                x = 17.65f,
                y = 6.41f,
                elevation = 0f
            };
        }

        public DivineFuryCharm() { }

        public override void ApplyEffects()
        {
            On.HeroController.CharmUpdate += OnUpdate;
        }

        /// <summary>
        /// When Divine Fury is equipped, set HP to 1 and 
        /// set Fury of the Fallen to trigger "automatically"
        /// </summary>
        private void OnUpdate(On.HeroController.orig_CharmUpdate orig, HeroController self)
        {
            // Perform charm updates first so that the health charms perform their effects
            orig(self);

            // Get Fury of the Fallen
            GameObject furyOfTheFallen = GameObject.Find("Charm Effects");
            if (SharedData.localSaveData.charmEquipped)
            {
                // Set health to 1 and blue health to 0
                // Doesn't affect Lifeblood Heart or Lifeblood Core
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
        /// Sets Fury of the Fallen to trigger automatically if 
        /// Fury of the Fallen is equipped
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
                    //SharedData.Log("Triggering Divine Fury");

                    fsm.ChangeTransition("Activate", "HERO HEALED FULL", "Stay Furied");
                    fsm.ChangeTransition("Stay Furied", "HERO HEALED FULL", "Activate");
                    fsm.Fsm.SendEventToFsmOnGameObject(furyOfTheFallen, "Fury", "HERO DAMAGED");
                }
                else
                {
                    // Reset FotF to trigger normally
                    //SharedData.Log("Deactivating Divine Fury");

                    fsm.ChangeTransition("Activate", "HERO HEALED FULL", "Deactivate");
                    fsm.ChangeTransition("Stay Furied", "HERO HEALED FULL", "Deactivate");
                    fsm.Fsm.SendEventToFsmOnGameObject(furyOfTheFallen, "Fury", "HERO HEALED FULL");
                }
            }
        }
    }
}