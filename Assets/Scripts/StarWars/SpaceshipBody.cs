using UnityEngine;
using StarWars.Brains;
using Avrahamy;
using Avrahamy.Audio;
using Avrahamy.Utils;

namespace StarWars {
    /// <summary>
    /// The visible body of the spaceship. Can move and rotate.
    /// </summary>
    public class SpaceshipBody : Body {
        [SerializeField] GameObject shield;
        [SerializeField] ParticleSystem energyEmitter;
        [SerializeField] AudioEvent shotSound;

        public SpaceshipBrain Brain {
            get {
                return brain;
            }
        }

        public AudioEvent ShotSound {
            get {
                return shotSound;
            }
        }

        public ParticleSystem EnergyEmitter {
            get {
                return energyEmitter;
            }
        }

        private SpaceshipBrain brain;
        private static ParticleSystem.Particle[] particles;

        protected void Awake() {
            brain = GetComponent<SpaceshipBrain>();

            DebugAssert.Assert(
                brain != null,
                name + " does not have a brain!\n" +
                "Did you add the _SpaceshipTemplate_ to the scene " +
                "or forgot to add your brain script to the prefab?");
        }

        public void Activate(
                Vector3 position,
                float angle,
                Space.Mutable space) {
            particles ??= new ParticleSystem.Particle[energyEmitter.main.maxParticles];
            Position = position;
            Rotation = angle;
            space.RegisterSpaceship(this, brain);

            name = brain.Name;
            gameObject.SetActive(true);
        }

        public void SetShield(bool isOn) {
            shield.SetActive(isOn);
        }

        protected override void OnTeleported() {
            // Teleporting the trail particles can cause a few particles to emit
            // in the middle of the screen in the next frame. Remove those.
            energyEmitter.Stop();
            this.DelaySeconds(ClearRogueParticles, Time.deltaTime);
        }

        private void ClearRogueParticles() {
            var particlesCount = energyEmitter.GetParticles(particles);
            var removedParticles = 0;
            for (int i = 0; i < particlesCount; i++) {
                var particle = particles[i];

                if (Mathf.Abs(particle.position.x) < Game.Size.x * 0.5f - 3f
                    && Mathf.Abs(particle.position.z) < Game.Size.y * 0.5f - 3f) {
                    // Remove particle.
                    ++removedParticles;
                    particle.remainingLifetime = -1f;
                } else if (removedParticles > 0) {
                    particles[i - removedParticles] = particle;
                }
            }
            energyEmitter.Play();
            energyEmitter.SetParticles(particles, particlesCount - removedParticles);
        }
    }
}
