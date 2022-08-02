using StarWars.Actions;

namespace StarWars.Brains {
    public class TwisterBrain : SpaceshipBrain {
        public override string DefaultName {
            get {
                return "Twister";
            }
        }

        public override IAction NextAction() {
            return spaceship.CanShoot ? Shoot.action : TurnRight.action;
        }
    }
}