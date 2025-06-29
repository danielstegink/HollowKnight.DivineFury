using ItemChanger.Locations;
using ItemChanger;
using UnityEngine;
using SFCore.Utils;
using GlobalEnums;
using System;
using Modding;
using System.Collections;

namespace DivineFury.Charms
{
    /// <summary>
    /// Divine Fury sets the player's health to 1
    /// </summary>
    public class DivineFuryCharm : Charm
    {
        public override string Name => GetName();

        public override string Description => GetDescription();

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

        public override string InternalName()
        {
            return "DivineFury";
        }

        /// <summary>
        /// Determines if the charm has been upgraded per Exaltation Expanded
        /// </summary>
        /// <returns></returns>
        public bool IsUpgraded()
        {
            return SharedData.localSaveData.charmUpgraded &&
                    SharedData.exaltationInstalled;
        }

        #region Get Data
        /// <summary>
        /// Gets charm name
        /// </summary>
        /// <returns></returns>
        private string GetName()
        {
            if (!IsUpgraded())
            {
                return "Divine Fury";
            }
            else
            {
                return "Godseeker's Lament";
            }
        }

        /// <summary>
        /// Gets charm description
        /// </summary>
        /// <returns></returns>
        private string GetDescription()
        {
            if (!IsUpgraded())
            {
                return "Manifestation of the Godseeker's outrage and hatred for trespassers.\n\n" +
                        "Reduces the bearer's health to 1.";
            }
            else
            {
                return "Manifestation of the Godseeker's fear and desperation.\n\n" +
                        "Reduces the bearer's health to 1, but gives a small chance to ignore damage.";
            }
        }
        #endregion

        public override void ApplyEffects()
        {
            On.HeroController.CharmUpdate += OnUpdate;
            ModHooks.TakeHealthHook += GodseekersLament;
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

        /// <summary>
        /// Godseeker's Lament has a small chance to ignore damage
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        private int GodseekersLament(int damage)
        {
            if (damage > 0 &&
                IsEquipped() &&
                IsUpgraded())
            {
                int random = UnityEngine.Random.Range(1, 101);
                int threshold = GetShieldChance();
                if (random <= threshold)
                {
                    //SharedData.Log("Ignoring damage");
                    HeroController.instance.StartCoroutine(ShieldFlash());
                    damage = 0;
                }
            }

            return damage;
        }

        /// <summary>
        /// Calculates the chance of ignoring damage
        /// </summary>
        /// <returns></returns>
        private int GetShieldChance()
        {
            // Carefree Melody shields for an average of about 7% per notch, so that will be our default
            int chance = 7;

            // There are several charms that don't work with this one. They improve healing, or they only
            //  trigger when damage is dealt. This mod will synergize with those charms by increasing
            //  its shield chance based on how expensive the equipped charms are
            int shieldPerNotch = 7;
            
            // Stalwart Shell - Its effect still triggers if damage is ignored.
            // Treat as half as valuable
            if (PlayerData.instance.equippedCharm_4)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_4 / 2;
            }

            // Grubsong - Triggers upon attack, even if damage ignored. 
            // Treat as half as valuable
            if (PlayerData.instance.equippedCharm_3)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_3 / 2;
            }

            // Fragile/Unbreakable Heart - Increases health by 2
            if (PlayerData.instance.equippedCharm_23)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_23;
            }

            // Thorns of Agony - Triggers upon attack, even if damage ignored. 
            // Treat as half as valuable
            if (PlayerData.instance.equippedCharm_12)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_12 / 2;
            }

            // Quick Focus - Increases healing speed
            if (PlayerData.instance.equippedCharm_7)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_7;
            }

            // Deep Focus - Increases healing amount
            if (PlayerData.instance.equippedCharm_34)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_34;
            }

            // Lifeblood Heart - Gives lifeblood masks
            if (PlayerData.instance.equippedCharm_8)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_8;
            }

            // Lifeblood Core - Gives lifeblood masks
            if (PlayerData.instance.equippedCharm_9)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_9;
            }

            // Joni's Blessing - Gives lifeblood masks
            if (PlayerData.instance.equippedCharm_27)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_27;
            }

            // Hiveblood - Gives passive healing
            if (PlayerData.instance.equippedCharm_29)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_29;
            }

            // Shape of Unn - Allows player to move while healing. Has some
            // combo potential with Sporeshroom, but largely useless
            if (PlayerData.instance.equippedCharm_28)
            {
                chance += shieldPerNotch * PlayerData.instance.charmCost_28;
            }

            // We don't want the shield chance to reach 100, so cap at 99%
            //SharedData.Log($"Final chance to ignore damage: {chance}%");
            chance = Math.Min(chance, 99);

            return chance;
        }

        /// <summary>
        /// Flashes the VFX to indicate damage was absorbed
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShieldFlash()
        {
            SpriteFlash flash = SharedData.GetField<HeroController, SpriteFlash>(HeroController.instance, "spriteFlash");
            flash.flashShadeGet();
            yield return new WaitForSeconds(0f);
        }
    }
}