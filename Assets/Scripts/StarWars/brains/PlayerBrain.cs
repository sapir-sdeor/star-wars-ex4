using UnityEngine.InputSystem;
using StarWars.Actions;

namespace StarWars.Brains {
    /// <summary>
    /// Controls:
    /// Right/Left/A/D to turn.
    /// Space/W to shoot.
    /// Hold Shift/S to keep shield up, release to drop the shield.
    /// </summary>
    public class PlayerBrain : SpaceshipBrain {
        public override string DefaultName {
            get {
                return "Player";
            }
        }

        private Keyboard keyboard;

        protected override void Awake() {
            base.Awake();
            keyboard = Keyboard.current;
        }

        public override IAction NextAction() {
            var wantsShield = keyboard.shiftKey.isPressed || keyboard.sKey.isPressed;
            if (wantsShield && spaceship.CanRaiseShield) {
                return ShieldUp.action;
            }
            if (!wantsShield && spaceship.IsShieldUp) {
                return ShieldDown.action;
            }
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) {
                return TurnRight.action;
            }
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) {
                return TurnLeft.action;
            }
            if ((keyboard.spaceKey.isPressed || keyboard.wKey.isPressed) && spaceship.CanShoot) {
                return Shoot.action;
            }
            return DoNothing.action;
        }
    }
}