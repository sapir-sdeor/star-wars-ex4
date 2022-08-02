// Author: Aviad & Ori
using StarWars.Actions;
using Avrahamy.Math;

namespace StarWars.Brains {
    public class DefenderBrain : SpaceshipBrain {
        public override string DefaultName {
            get {
                return "Defender";
            }
        }

        private Spaceship target = null;
        private Spaceship chaser = null;

        /// <summary>
        /// If the defender feels attacked - it turns on the shield if it can,
        /// otherwise it starts circling right.
        /// The defender selects the closest ship as target and tries to shoot it.
        /// </summary>
        public override IAction NextAction() {
            // Find current target and chaser.
            var distance = float.MaxValue;
            foreach (var ship in Space.Spaceships) {
                // Make sure not to target self or dead spaceships and choose the closest one.
                if (spaceship != ship && ship.IsAlive) {
                    if (spaceship.ClosestRelativePosition(ship).magnitude < distance) {
                        target = ship;
                        distance = spaceship.ClosestRelativePosition(ship).magnitude;
                    }
                    var angle = ship.Forward.GetAngleBetweenXZ(ship.ClosestRelativePosition(spaceship));
                    if (angle < 10f && angle > -10f) {
                        chaser = ship;
                    }
                }
            }

            if (chaser != null && (!spaceship.IsShieldUp) &&
                    spaceship.ClosestRelativePosition(chaser).magnitude < 6f){
                return spaceship.CanRaiseShield ? ShieldUp.action : TurnRight.action;
            }

            if (target != null) {
                // Try to kill it.
                var pos = spaceship.ClosestRelativePosition(target);
                var forwardVector = spaceship.Forward;
                var angle = pos.GetAngleBetweenXZ(forwardVector);
                if (angle >= 10f) return TurnLeft.action;
                if (angle <= -10f) return TurnRight.action;
                if (distance < 20f && (!target.IsShieldUp || target.Energy < 3)) {
                    return spaceship.CanShoot ? Shoot.action : DoNothing.action;
                }
            }
            return DoNothing.action;
        }
    }
}