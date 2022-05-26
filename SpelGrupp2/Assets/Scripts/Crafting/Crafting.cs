using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CallbackSystem
{
    /*
     * Where the players inventory is instantiated, stored and managed.
     * Calls on method in PlayerAttack when bullet is crafted. 
     */
    public class Crafting : MonoBehaviour
    {
        [HideInInspector] public PlayerAttack playerAttackScript;
        [SerializeField]
        private Recipe batteryRecipe, bulletRecipe,
        UpgradedProjectileWeaponRecipe, UpgradedLaserWeaponRecipe,
        cyanRecipe, yellowRecipe, whiteRecipe, magentaRecipe,
        greenRecipe, blackRecipe, RevolverCritRecipe, laserbeamWidthRecipe,
        largeMagazineRecipe, laserbeamChargeRecipe;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private GameObject craftingTable;
        [SerializeField] private Button[] craftingButtons;
        [SerializeField] private Button defaultColorButton;
        private int[] resourceArray;
        private Button selectedButton;
        private float sphereRadius = 1f;
        private float maxSphereDistance = 3f;
        private int selectedButtonIndex;
        //Cyan, Yellow, Magenta, White, Black
        private static bool[] colorsTakenArray = new bool[5];

        public int copper, transistor, iron, currency;

        private ResourceUpdateEvent resourceEvent;
        private FadingTextEvent fadingtextEvent;
        private PlayerHealth playerHealthScript;
        private bool isPlayerOne, started = false, isCrafting = false;
        private PlayerInput playerInput;

        public void UpdateResources()
        {
            resourceEvent.c = copper;
            resourceEvent.t = transistor;
            resourceEvent.i = iron;
            resourceEvent.currency = currency;
            resourceEvent.ammoChange = false;
            resourceArray = new int[] { copper, transistor, iron, currency };
            EventSystem.Current.FireEvent(resourceEvent);
        }


        public bool IsPlayerOne() { return isPlayerOne; }

        private void Awake()
        {
            playerAttackScript = GetComponent<PlayerAttack>();
            playerHealthScript = GetComponent<PlayerHealth>();
            playerInput = GetComponent<PlayerInput>();
            fadingtextEvent = new FadingTextEvent();
            resourceEvent = new ResourceUpdateEvent();
            craftingTable.SetActive(false);
            resourceArray = new int[] { copper, transistor, iron, currency };

        }

        private void Update()
        {
            if (!started)
            {
                isPlayerOne = playerAttackScript.IsPlayerOne();
                resourceEvent.isPlayerOne = isPlayerOne;
                fadingtextEvent.isPlayerOne = isPlayerOne;
                UpdateResources();
                started = true;
            }
        }

        //%-------------------------------Crafting table----------------------------------%

        public void Interact(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (isCrafting)
                    EnterCraftingUI();
                else
                {
                    RaycastHit hit;
                    Physics.SphereCast(transform.position, sphereRadius, transform.forward,
                    out hit, maxSphereDistance, layerMask);

                    if (hit.collider != null)
                    {
                        if (hit.transform.tag == "CraftingTable")
                        {
                            EnterCraftingUI();
                        }
                        else if (hit.transform.tag == "Generator")
                        {
                            GeneratorEvent generator = hit.transform.GetComponent<GeneratorEvent>();
                            generator.StartGenerator();
                        }
                        else if (hit.transform.tag == "Exit")
                        {
                            OpenExit exit = hit.transform.GetComponentInParent<OpenExit>();
                            exit.OpenDoor();
                        }
                    }
                }
            }
        }

        private void EnterCraftingUI()
        {
            isCrafting = !isCrafting;

            if (!isCrafting)
            {
                craftingTable.SetActive(false);
                selectedButton.image.color = Color.white;
                selectedButtonIndex = 0;
                playerInput.SwitchCurrentActionMap("Player");
            }
            else
            {
                craftingTable.SetActive(true);
                selectedButton = craftingButtons[selectedButtonIndex];
                selectedButton.image.color = Color.red;
                playerInput.SwitchCurrentActionMap("Crafting");
            }
        }

        public void NextButton(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                selectedButtonIndex++;
                if (selectedButtonIndex == craftingButtons.Length)
                    selectedButtonIndex = 0;

                ChangeSelectedButton();
            }
        }

        public void PreviousButton(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                selectedButtonIndex--;
                if (selectedButtonIndex < 0)
                    selectedButtonIndex = craftingButtons.Length - 1;

                ChangeSelectedButton();
            }
        }

        private void ChangeSelectedButton()
        {
            selectedButton.image.color = Color.white;
            selectedButton = craftingButtons[selectedButtonIndex];
            selectedButton.image.color = Color.red;
        }

        public void SelectButton(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (selectedButton.interactable)
                    selectedButton.onClick.Invoke();
                else
                {
                    fadingtextEvent.text = "Unavailable Purchase";
                    EventSystem.Current.FireEvent(fadingtextEvent);
                }

            }
        }

        //%--------------------------------Crafts & upgrades---------------------------------%

        public void CraftBullet(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (playerAttackScript.ReturnBullets() < playerAttackScript.ReturnMaxBullets())
                {
                    if (TryCraftRecipe(bulletRecipe))
                    {
                        playerAttackScript.UpdateBulletCount(3);
                        fadingtextEvent.text = "Bullets Crafted (x3)";
                        EventSystem.Current.FireEvent(fadingtextEvent);
                    }
                    else
                    {
                        Debug.Log("Carrying max bullets!");
                    }
                }
            }
        }
        public void CraftBattery(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (playerHealthScript.GetCurrentBatteryCount() < playerHealthScript.GetMaxBatteryCount())
                {
                    if (TryCraftRecipe(batteryRecipe))
                    {
                        playerHealthScript.IncreaseBattery();
                        fadingtextEvent.text = "Battery Crafted";
                        EventSystem.Current.FireEvent(fadingtextEvent);
                    }
                }
                else
                {
                    Debug.Log("Carrying max batteries!");
                }
            }
        }

        public void CraftUpgradedProjectileWeapon()
        {
            if (TryCraftRecipe(UpgradedProjectileWeaponRecipe))
            {
                playerAttackScript.UpgradeProjectileWeapon();
                fadingtextEvent.text = "Revolver damage Upgraded";
                selectedButton.interactable = false;
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftCrittableRevolver()
        {
            if (TryCraftRecipe(RevolverCritRecipe))
            {
                //playerAttackScript.UpgradeRevolverCrittable();
                fadingtextEvent.text = "Revolver Crit Enabled";
                selectedButton.interactable = false;
            }     
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftUpgradedRevolverAmmo()
        {
            if (TryCraftRecipe(largeMagazineRecipe))
            {
                playerAttackScript.MagSizeUpgrade();
                fadingtextEvent.text = "Revolver Ammo Upgraded";
                selectedButton.interactable = false;
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }


        public void CraftReducedBeamCharge()
        {
            if (TryCraftRecipe(laserbeamChargeRecipe))
            {
                playerAttackScript.ChargeRateUpgrade();
                fadingtextEvent.text = "Lasergun Upgraded";
                selectedButton.interactable = false;
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftIncreaseBeamWidth()
        {
            if (TryCraftRecipe(laserbeamWidthRecipe))
            {
                playerAttackScript.BeamWidthUpgrade();
                fadingtextEvent.text = "Lasergun Upgraded";
                selectedButton.interactable = false;
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftUpgradedLaserWeapon()
        {
            if (TryCraftRecipe(UpgradedLaserWeaponRecipe))
            {
                playerAttackScript.UpgradeLaserWeapon();
                fadingtextEvent.text = "Lasergun Upgraded";
                selectedButton.interactable = false;
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        //%-----------------------------------Colors------------------------------------%

        public void CraftMaterialColorCyan()
        {
            if (TryCraftRecipe(cyanRecipe))
            {
                playerHealthScript.ChooseMaterialColor(new Color(0.1f, 0.90f, 0.90f, 1f));
                fadingtextEvent.text = "Color Cyan Crafted";
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }
        public void CraftMaterialColorYellow()
        {
            if (TryCraftRecipe(yellowRecipe))
            {
                playerHealthScript.ChooseMaterialColor(Color.yellow);
                fadingtextEvent.text = "Color Yellow Crafted";
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }
        public void CraftMaterialColorWhite()
        {
            if (TryCraftRecipe(whiteRecipe))
            {
                playerHealthScript.ChooseMaterialColor(new Color(0.95f, 0.95f, 0.95f, 1f));
                fadingtextEvent.text = "Color White Crafted";
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }
        public void CraftMaterialColorMagenta()
        {
            if (TryCraftRecipe(magentaRecipe))
            {
                playerHealthScript.ChooseMaterialColor(new Color(0.85f, 0f, 0.85f, 1f));
                fadingtextEvent.text = "Color Magenta Crafted";
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftMaterialColorGreen()
        {
            if (TryCraftRecipe(greenRecipe))
            {
                playerHealthScript.ChooseMaterialColor(new Color(0.35f, 0.95f, 0f, 1f));
                fadingtextEvent.text = "Color Green Crafted";
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftMaterialColorBlack()
        {
            if (TryCraftRecipe(blackRecipe))
            {
                playerHealthScript.ChooseMaterialColor(new Color(0.25f, 0.25f, 0.25f, 1f));
                fadingtextEvent.text = "Color Black Crafted";
            }
            else
                fadingtextEvent.text = "Not Enough Resources";
            EventSystem.Current.FireEvent(fadingtextEvent);
        }

        public void CraftDefaultColor()
        {
            playerHealthScript.ChooseMaterialColor();
            fadingtextEvent.text = "Default Color Crafted";
        }

        //%-----------------------------------------------------------------------------%
        public bool TryCraftRecipe(Recipe recipe)
        {
            bool missingResources = false;
            if (recipe == null) Debug.LogWarning("Trying to craft null");

            for (int i = 0; i < recipe.ResNeededArr.Length; i++)
            {
                if (resourceArray[i] < recipe.ResNeededArr[i])
                {
                    Debug.Log("Not enough resources");
                    missingResources = true;
                }
            }

            if (!missingResources)
            {
                copper -= recipe.copperNeeded;
                iron -= recipe.ironNeeded;
                transistor -= recipe.transistorNeeded;
                currency -= recipe.currencyNeeded;
                UpdateResources();
                if (selectedButton != defaultColorButton && isCrafting)
                    selectedButton.interactable = false;
                return true;
            }
            return false;
        }
    }
}