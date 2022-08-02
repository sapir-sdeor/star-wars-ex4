using UnityEngine;
using StarWars.Actions;

namespace StarWars.Brains {
    public abstract class SpaceshipBrain : MonoBehaviour {
        protected Spaceship spaceship;

        protected virtual void Awake() {
            Name = DefaultName;
        }

        public void Activate(Spaceship spaceship) {
            this.spaceship = spaceship;
        }

        /// <summary>
        /// Reset is called when a component is added to a game object (also in the
        /// editor).
        /// </summary>
        protected void Reset() {
            name = DefaultName + "Spaceship";
        }

        public string Name { get; set; }
        public abstract string DefaultName { get; }
        public abstract IAction NextAction();
    }
}