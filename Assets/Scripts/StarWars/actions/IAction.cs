namespace StarWars.Actions {
    public interface IAction {
        void Do(Spaceship.Mutable spaceship);
        bool CanDo(Spaceship spaceship);
    }
}