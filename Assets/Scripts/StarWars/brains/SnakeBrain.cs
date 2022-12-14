// Author: Nadav
using UnityEngine;
using StarWars.Actions;
using Avrahamy.Math;

namespace StarWars.Brains {
    public class SnakeBrain : SpaceshipBrain {
        public override string DefaultName {
            get {
                return "Snake";
            }
        }

        [Range(0f, 1f)]
        [SerializeField] float rotationChance = 0.03f;

        private IAction turnDirection;

        protected override void Awake() {
            base.Awake();
            ChooseDirection();
        }

        /// <summary>
        /// The snake make sharp turns left and right and shoots whenever it can.
        /// </summary>
        public override IAction NextAction() {
            // Make sure to move in a stright line.
            var rotation = spaceship.Rotation;
            // Rotation is in specific degrees so we won't allways be exactly 90
            // degrees.
            var modAngle = MathsUtils.Mod(rotation, 90f);
            if (modAngle > 45f) {
                modAngle = 90f - modAngle;
            }
            if (modAngle >= Spaceship.ROTATION_PER_ACTION * 0.7f) {
                return turnDirection;
            }

            // Checks if we should rotate.
            if (Random.value < rotationChance) {
                // Initialize Rotation.
                ChooseDirection();
                return turnDirection;
            }

            return spaceship.CanShoot ? Shoot.action : DoNothing.action;
        }

        private void ChooseDirection() {
            if (Random.value > 0.5f) {
                turnDirection = TurnLeft.action;
            } else {
                turnDirection = TurnRight.action;
            }
        }
    }
}