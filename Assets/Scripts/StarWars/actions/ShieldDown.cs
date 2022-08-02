namespace StarWars.Actions {
    public class ShieldDown : IAction {
        public static readonly IAction action = new ShieldDown();

        public void Do(Spaceship.Mutable spaceship) {
            spaceship.ShieldDown();
        }

        public bool CanDo(Spaceship spaceship) {
            return spaceship.IsShieldUp;
        }
    }
}