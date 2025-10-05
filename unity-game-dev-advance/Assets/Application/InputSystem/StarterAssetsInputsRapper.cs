using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable InconsistentNaming

namespace Unity_Game_Dev_Advanced
{
    /// <summary>
    /// Rapper StarterAssetInputs Class 
    /// </summary>
    public class StarterAssetsInputsRapper : StarterAssetsInputs
    {
        [Header("Custom Input Settings")]
        public bool attack;
        public bool interactive;

        private void Awake()
        {
            attack = false;
            interactive = false;
        }

#if ENABLE_INPUT_SYSTEM
        public new void OnMove(InputValue value)
        {
            if(DisableMoveInput()) return;
            
            MoveInput(value.Get<Vector2>());
        }

        public new void OnLook(InputValue value)
        {
            if(cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public new void OnJump(InputValue value)
        {
            if(DisableMoveInput()) return;
            
            JumpInput(value.isPressed);
        }

        public new void OnSprint(InputValue value)
        {
            if(DisableMoveInput()) return;
            
            SprintInput(value.isPressed);
        }

        public void OnAttack(InputValue value)
        {
            if(interactive) return;
            if(jump) return;

            sprint = false;
            move = Vector2.zero;
            
            attack = value.isPressed;
        }

        public void OnInteractive(InputValue value)
        {
            if(attack) return;
            if(jump) return;
            
            sprint = false;
            move = Vector2.zero;
            
            interactive = value.isPressed;
        }
#endif

        private bool DisableMoveInput()
        {
            return attack || interactive;
        }
    }
}