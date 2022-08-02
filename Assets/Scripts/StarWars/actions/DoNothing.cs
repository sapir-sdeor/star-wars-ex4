namespace StarWars.Actions {
    public class DoNothing : IAction {
        public static readonly IAction action = new DoNothing();

        public void Do(Spaceship.Mutable spaceship) {
            // Empty on purpose.
        }

        public bool CanDo(Spaceship spaceship) {
            return true;
        }
    }
}