using System;
using Stariluz;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stariluz
{
    public class PlayerWithAxeMovement : MonoBehaviour
    {
        protected PlayerMovement player;
        protected int AClayerIndex;
        [SerializeField] protected bool hasAxeEquipped = false;
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
            AClayerIndex = player.animator.GetLayerIndex("Axe");
        }

        void Update()
        {
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            float targetWeight = hasAxeEquipped ? 1f : 0f;
            float smoothedSpeed = Mathf.SmoothDamp(
                player.animator.GetLayerWeight(AClayerIndex),
                targetWeight,
                ref layerWeightVelocity,
                Time.deltaTime * player.smoothTime
                );

            player.animator.SetLayerWeight(AClayerIndex, smoothedSpeed);
        }

        private void EquipAxe(InputAction.CallbackContext context)
        {
            hasAxeEquipped = !hasAxeEquipped;
        }
    }
}
