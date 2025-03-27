using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stariluz
{
    public class PlayerWithWeaponMovement : MonoBehaviour
    {
        protected PlayerMovement player;
        protected int ACweaponLayerIndex;
        protected int ACweaponDrawLayerIndex;
        protected int ACdrawWeaponHash;
        protected int ACsheathWeaponHash;
        [SerializeField] protected bool isWeaponDrawn = false;
        private float layerWeightVelocity = 0f;

        private void OnEnable()
        {
            player = GetComponent<PlayerMovement>();
            player.playerInput.EquipWeapon.started += EquipAxe;
        }
        private void OnDisable()
        {
            player.playerInput.EquipWeapon.started -= EquipAxe;
        }

        void Start()
        {
            ACweaponLayerIndex = player.animator.GetLayerIndex("Weapon");
            ACweaponDrawLayerIndex = player.animator.GetLayerIndex("WeaponDraw");
            ACdrawWeaponHash = Animator.StringToHash("drawWeapon");
            ACsheathWeaponHash = Animator.StringToHash("sheathWeapon");
        }

        void Update()
        {
            if (!isWeaponDrawn)
            {
                UpdateEquipAnimation();
            }
            else
            {
                UpdateNormalAnimation();
            }
        }
        private void UpdateNormalAnimation()
        {
            UpdateAnimation(player.smoothTime);
        }

        private void UpdateEquipAnimation()
        {
            UpdateAnimation(player.smoothTime * 8);
        }

        private void UpdateAnimation(float smoothTime)
        {
            float targetWeight = isWeaponDrawn ? 1f : 0f;
            float smoothedSpeed = Mathf.SmoothDamp(
                player.animator.GetLayerWeight(ACweaponLayerIndex),
                targetWeight,
                ref layerWeightVelocity,
                Time.deltaTime * smoothTime
                );

            player.animator.SetLayerWeight(ACweaponLayerIndex, smoothedSpeed);
        }
        private void EquipAxe(InputAction.CallbackContext context)
        {
            isWeaponDrawn = !isWeaponDrawn;
            if (isWeaponDrawn)
            {
                player.animator.SetTrigger(ACdrawWeaponHash);
            }
            else
            {
                player.animator.SetTrigger(ACsheathWeaponHash);
            }
        }
    }
}
