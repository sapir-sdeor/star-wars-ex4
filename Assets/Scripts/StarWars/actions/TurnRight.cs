namespace StarWars.Actions {
    public class TurnRight : IAction {
        public static readonly IAction action = new TurnRight();

        public void Do(Spaceship.Mutable spaceship) {
            spaceship.Rotation += Spaceship.ROTATION_PER_ACTION;
        }

        public bool CanDo(Spaceship spaceship) {
            return spaceship.IsAlive;
        }
    }
}