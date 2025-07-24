using GlobalEnums;
using ItemChanger;
using ItemChanger.Locations;
using SFCore.Utils;
using System;
using System.Collections;
using UnityEngine;

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
            On.HeroController.TakeDamage += GodseekersLament;
        }

        #region Divine Fury
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
        #endregion

        #region Godseeker's Lament
        /// <summary>
        /// Checks if the player is currently immune to damage
        /// </summary>
        private bool isImmune = false;

        /// <summary>
        /// Godseeker's Lament has a small chance to ignore damage
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="self"></param>
        /// <param name="go"></param>
        /// <param name="damageSide"></param>
        /// <param name="damageAmount"></param>
        /// <param name="hazardType"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void GodseekersLament(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, CollisionSide damageSide, 
                                        int damageAmount, int hazardType)
        {
            if (CanTakeDamage(hazardType) &&
                damageAmount > 0 &&
                IsEquipped() &&
                IsUpgraded())
            {
                // Check if currently immune
                if (isImmune)
                {
                    damageAmount = 0;
                }
                else // Otherwise, run the numbers
                {
                    int random = UnityEngine.Random.Range(1, 101);
                    int threshold = GetShieldChance();
                    //SharedData.Log($"Attempting to block: {random} vs {threshold}");
                    if (random <= threshold)
                    {
                        damageAmount = 0;
                        isImmune = true;
                        GameManager.instance.StartCoroutine(ShieldFlash());
                    }
                }
            }

            orig(self, go, damageSide, damageAmount, hazardType);
        }

        /// <summary>
        /// Determines if the player can take damage
        /// </summary>
        /// <param name="hazardType"></param>
        /// <returns></returns>
        private bool CanTakeDamage(int hazardType)
        {
            // Damage is calculated by the HeroController's TakeDamage method
            // First, run the CanTakeDamage check from the HeroController
            bool canTakeDamage = SharedData.CallFunction<HeroController, bool>(HeroController.instance, "CanTakeDamage", null);
            if (canTakeDamage)
            {
                // There is an additional check for I-Frames and shadow dashing
                if (hazardType == 1)
                {
                    if (HeroController.instance.damageMode == DamageMode.HAZARD_ONLY ||
                        HeroController.instance.cState.shadowDashing ||
                        HeroController.instance.parryInvulnTimer > 0f)
                    {
                        canTakeDamage = false;
                    }
                }
                else if (HeroController.instance.cState.invulnerable ||
                            PlayerData.instance.isInvincible)
                {
                    canTakeDamage = false;
                }
            }

            return canTakeDamage;
        }

        /// <summary>
        /// Calculates the chance of ignoring damage
        /// </summary>
        /// <returns></returns>
        private int GetShieldChance()
        {
            // Carefree Melody shields for an average of about 7% per notch
            int chance = 7;

            // There are several charms that don't work with this one. They improve healing, or they only
            //  trigger when damage is dealt. This mod will synergize with those charms by increasing
            //  its shield chance
            int multiplier = 1;

            // Stalwart Shell - Its effect still triggers if damage is ignored.
            // Grubsong - Triggers upon attack, even if damage ignored. 
            // Thorns of Agony - Triggers upon attack, even if damage ignored. 

            // Fragile/Unbreakable Heart - Increases health by 2
            if (PlayerData.instance.equippedCharm_23)
            {
                multiplier++;
            }

            // Quick Focus - Increases healing speed
            if (PlayerData.instance.equippedCharm_7)
            {
                multiplier++;
            }

            // Deep Focus - Increases healing amount
            if (PlayerData.instance.equippedCharm_34)
            {
                multiplier++;
            }

            // Hiveblood - Gives passive healing
            if (PlayerData.instance.equippedCharm_29)
            {
                multiplier++;
            }

            // Shape of Unn - Allows player to move while healing. Has some
            // combo potential with Sporeshroom, but largely useless
            if (PlayerData.instance.equippedCharm_28)
            {
                multiplier++;
            }

            // Exalted lifeblood charms give extra health, so they will be excluded from this
            // Lifeblood Heart - Gives lifeblood masks
            // Lifeblood Core - Gives lifeblood masks
            // Joni's Blessing - Gives lifeblood masks

            // We don't want the shield chance to go out of control, so prevent it from reaching 100%
            //SharedData.Log($"Final chance to ignore damage: {chance}%");
            chance *= multiplier;
            chance = Math.Min(chance, 99);

            return chance;
        }

        /// <summary>
        /// Flashes the VFX to indicate damage was absorbed. Also ensures player stays immune for 
        /// a short time in case the damage was caused by collision with an enemy.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShieldFlash()
        {
            // Want to wait 1 second before turning immunity off, and want the VFX to stay active
            //  for about that long to show that the charm activated
            SpriteFlash flash = SharedData.GetField<HeroController, SpriteFlash>(HeroController.instance, "spriteFlash");
            flash.flash(UnityEngine.Color.black, 0.8f, 0.4f, 0.5f, 0.4f);

            yield return new WaitForSeconds(1.3f);
            isImmune = false;
        }
        #endregion
    }
}