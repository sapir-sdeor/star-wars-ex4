namespace StarWars.Actions {
    public class ShieldUp : IAction {
        public static readonly IAction action = new ShieldUp();

        public void Do(Spaceship.Mutable spaceship) {
            spaceship.ShieldUp();
        }

        public bool CanDo(Spaceship spaceship) {
            return spaceship.CanRaiseShield;
        }
    }
}