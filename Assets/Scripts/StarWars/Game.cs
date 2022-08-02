using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Avrahamy;
using Avrahamy.Audio;
using Avrahamy.Collections;
using StarWars.UI;

namespace StarWars {
    public class Game : MonoBehaviour {
        public enum GameState {
            PreMatch,
            Match,
            PostMatch,
        }

        public const int SCORE_FOR_BASHING = 5;
        public const int SCORE_FOR_SHOOTING = 3;
        public const int SCORE_FOR_SURVIVING = 1;
        public const int SCORE_FOR_TIE = 1;
        private readonly Space.Mutable space = new Space.Mutable();

        [SerializeField] GameState state;

        [Header("Pre Match")]
        [SerializeField] PassiveTimer preMatchDelay;

        [Header("Winner")]
        [SerializeField] Transform winnerCamera;
        [SerializeField] Text winnerText;
        [SerializeField] PassiveTimer winnerDelay;

        [Header("Tie")]
        [SerializeField] GameObject tieText;
        [SerializeField] PassiveTimer tieDelay;
        [SerializeField] PassiveTimer suddenDeathTimer;
        [SerializeField] Text suddenDeathTimerText;

        [Header("Score")]
        [SerializeField] int livesPerShip = 1;
        [SerializeField] ScoreBoard scoreBoard;

        [Header("Pools")]
        [SerializeField] GameObjectPool shotBodiesPool;
        [SerializeField] GameObjectPool explosionEffectsPool;
        [SerializeField] GameObjectPool respawnEffectsPool;

        [Header("Audio")]
        [SerializeField] AudioEvent killSound;
        [SerializeField] AudioEvent collisionSound;
        [SerializeField] AudioEvent shieldBurnShipSound;
        [SerializeField] AudioEvent shieldBurnShotSound;
        [SerializeField] AudioEvent shieldUpSound;

        private readonly HashSet<Spaceship.Mutable> deadShips = new HashSet<Spaceship.Mutable>();
        private readonly HashSet<Shot.Mutable> deadShots = new HashSet<Shot.Mutable>();

        private static Game instance;

        public static Vector2 Size {
            get {
                if (worldSize.sqrMagnitude < 1f) {
#if UNITY_EDITOR
                    var classType = Type.GetType("UnityEditor.GameView,UnityEditor");
                    var GetSizeOfMainGameView = classType.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    var result = (Vector2)GetSizeOfMainGameView.Invoke(null,null);
                    var resolution = new Resolution {
                        width = (int)result.x,
                        height = (int)result.y
                    };
                    var res = resolution;
#else
                    var res Screen.currentResolution;
#endif

                    var worldTopRight = Camera.main.ScreenToWorldPoint(new Vector3(res.width, res.height));
                    var worldBottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
                    var cameraWorldSize = worldTopRight - worldBottomLeft;
                    // Compensate for the 45 degree angle of the camera.
                    worldSize.x = cameraWorldSize.x + 1.3f;
                    worldSize.y = cameraWorldSize.y * 2f + 1.3f;
                }
                return worldSize;
            }
        }

        public static int LivesPerShip {
            get {
                return instance.livesPerShip;
            }
        }

        private static Vector2 worldSize;
        private SpaceshipBody[] spaceshipBodies;
        private List<Spaceship.Mutable> activeShips;
        private List<Spaceship.Mutable> lastActiveShips;
        private Keyboard keyboard;

        protected void Awake() {
#if UNITY_EDITOR
            // Make sure the game is played in the correct resolution.
            var sizeIndex = Avrahamy.EditorGadgets.GameViewUtils.FindSize(
                UnityEditor.GameViewSizeGroupType.Standalone,
                1280,
                800);
            if (sizeIndex < 0) {
                Avrahamy.EditorGadgets.GameViewUtils.AddCustomSize(
                    Avrahamy.EditorGadgets.GameViewUtils.GameViewSizeType.FixedResolution,
                    UnityEditor.GameViewSizeGroupType.Standalone,
                    1280,
                    800,
                    "Star Wars");
                sizeIndex = Avrahamy.EditorGadgets.GameViewUtils.FindSize(
                    UnityEditor.GameViewSizeGroupType.Standalone,
                    1280,
                    800);
            }
            Avrahamy.EditorGadgets.GameViewUtils.SetSize(sizeIndex);
#endif

            instance = this;
            keyboard = Keyboard.current;

            activeShips = new List<Spaceship.Mutable>();
        }

        protected void Start() {
            var nameCount = new Dictionary<string, int>();
            spaceshipBodies = FindObjectsOfType<SpaceshipBody>();

            DebugAssert.Assert(0 < spaceshipBodies.Length, "Add Spaceships to the scene");
            DebugAssert.Assert(spaceshipBodies.Length <= 6, "Max 6 spaceships allowed!");

            foreach (var body in spaceshipBodies) {
                // Check for duplicate names.
                var brain = body.Brain;
                if (nameCount.ContainsKey(brain.DefaultName)) {
                    ++nameCount[brain.DefaultName];
                    brain.Name = brain.DefaultName + " " + nameCount[brain.DefaultName];
                } else {
                    nameCount[brain.DefaultName] = 1;
                }

                // Register to score board.
                scoreBoard.Add(brain.Name);
            }

            StartMatch();
        }

        private void StartMatch() {
            space.ClearSpace();

            // Position spaceships.
            foreach (var body in spaceshipBodies) {
                var spawnPoint = Space.GetSpawnPoint();
                var angle = UnityEngine.Random.Range(1, 360 / Spaceship.ROTATION_PER_ACTION) * Spaceship.ROTATION_PER_ACTION;
                body.Activate(spawnPoint, angle, space);
            }
            lastActiveShips = new List<Spaceship.Mutable>(space.Spaceships);

            state = GameState.PreMatch;
            preMatchDelay.Start();

            winnerCamera.SetParent(null, false);
            winnerCamera.gameObject.SetActive(false);
            tieText.SetActive(false);
        }

        protected void Update() {
            if (keyboard.minusKey.wasPressedThisFrame || keyboard.numpadMinusKey.wasPressedThisFrame) {
                if (Time.timeScale > 0.5f) {
                    Time.timeScale = 0.5f;
                } else if (Time.timeScale > 0.2f) {
                    Time.timeScale *= 0.4f;
                } else {
                    Time.timeScale = 0.1f;
                }
                DebugLog.LogError("Time Scale: " + Time.timeScale);
            }
            if (keyboard.numpadPlusKey.wasPressedThisFrame || keyboard.equalsKey.wasPressedThisFrame
                || keyboard.numpadEqualsKey.wasPressedThisFrame) {
                if (Time.timeScale < 0.5f) {
                    Time.timeScale *= 2.5f;
                } else {
                    Time.timeScale = 1f;
                }
                DebugLog.LogError("Time Scale: " + Time.timeScale);
            }

            switch (state) {
                case GameState.PreMatch:
                    if (!preMatchDelay.IsSet || preMatchDelay.IsActive) return;
                    state = GameState.Match;
                    preMatchDelay.Clear();
                    suddenDeathTimer.Start();
                    break;
                case GameState.PostMatch:
                    if (winnerDelay.IsSet) {
                        if (!winnerDelay.IsActive) {
                            var winner = activeShips[0];
                            winnerCamera.SetParent(winner.Transform, false);
                            winnerCamera.gameObject.SetActive(true);
                            winnerText.text = "WINNER\n" + winner.Name.ToUpper();
                            winnerDelay.Clear();
                        }
                    } else if (tieDelay.IsSet) {
                        if (!tieDelay.IsActive) {
                            tieText.SetActive(true);
                            tieDelay.Clear();
                            suddenDeathTimerText.gameObject.SetActive(false);
                        }
                    } else if (keyboard.enterKey.wasPressedThisFrame) {
                        StartMatch();
                    }
                    break;
                case GameState.Match:
                    if (!suddenDeathTimer.IsActive) return;
                    if (suddenDeathTimer.RemainingTime <= 10.09f) {
                        suddenDeathTimerText.gameObject.SetActive(true);
                        suddenDeathTimerText.text = suddenDeathTimer.RemainingTime.ToString("N2");
                    } else {
                        suddenDeathTimerText.gameObject.SetActive(false);
                    }
                    break;
            }
        }

        protected void FixedUpdate() {
            if (state == GameState.PreMatch) return;

            activeShips.Clear();
            var spaceships = space.Spaceships;
            // First all alive spaceships select their next action.
            foreach (var spaceship in spaceships) {
                if (spaceship.LivesRemaining <= 0) continue;
                activeShips.Add(spaceship);
                if (spaceship.IsAlive) {
                    spaceship.SelectAction();
                }
            }
            // Then all spaceships do their turn.
            foreach (var spaceship in spaceships) {
                spaceship.DoTurn();
            }

            var shots = space.Shots;
            foreach (var shot in shots) {
                if (shot.IsAlive) {
                    shot.DoTurn();
                }
            }

            deadShips.Clear();
            deadShots.Clear();
            // Check for collisions.
            // Spaceship to spaceship.
            for (int i = 0; i < spaceships.Count - 1; i++) {
                var ship = spaceships[i];
                if (!ship.IsAlive) continue;
                for (int j = i + 1; j < spaceships.Count; j++) {
                    var other = spaceships[j];
                    if (!other.IsAlive) continue;
                    if (ship.obj.CheckCollision(other.obj)) {
                        DebugLog.Log(ship.Name + " collided with ship " + other.Name);
                        if (ship.IsShieldUp != other.IsShieldUp) {
                            // The ship without the shield is dead.
                            var killer = ship.IsShieldUp ? ship : other;
                            var dead = ship.IsShieldUp ? other : ship;
                            scoreBoard.AddScore(killer.Name, SCORE_FOR_BASHING);
                            SpaceshipKilled(dead);
                            shieldBurnShipSound.Play();
                            if (ship == dead) break;
                        } else {
                            SpaceshipKilled(other);
                            SpaceshipKilled(ship);
                            collisionSound.Play();
                            break;
                        }
                    }
                }
            }
            // Shot to spaceship.
            foreach (var ship in spaceships) {
                if (!ship.IsAlive) continue;
                foreach (var shot in shots) {
                    if (!shot.IsAlive) continue;
                    if (shot.obj.CheckCollision(ship.obj)) {
                        DebugLog.Log(ship.Name + " collided with shot " + shot.Name);
                        if (!ship.IsShieldUp) {
                            ship.Health -= Shot.DAMAGE;
                            if (!ship.IsAlive) {
                                scoreBoard.AddScore(shot.Shooter.Name, SCORE_FOR_SHOOTING);
                                SpaceshipKilled(ship);
                                killSound.Play();
                            } else {
                                // TODO: Add hit sound.
                            }
                        } else {
                            shieldBurnShotSound.Play();
                        }
                        shot.BeDead();
                        break;
                    }
                }
            }

            // Clear dead ships.
            foreach (var spaceship in spaceships) {
                if (!spaceship.IsAlive) {
                    deadShips.Add(spaceship);
                }
            }
            foreach (var spaceship in deadShips) {
                space.RemoveSpaceship(spaceship);
            }

            // Clear dead shots.
            foreach (var shot in shots) {
                if (!shot.IsAlive) {
                    deadShots.Add(shot);
                }
            }
            foreach (var shot in deadShots) {
                space.RemoveShot(shot);
            }

            if (state != GameState.Match) return;

            if (activeShips.Count == 1) {
                state = GameState.PostMatch;
                winnerDelay.Start();
                var winner = activeShips[0];
                scoreBoard.AddScore(winner.Name, SCORE_FOR_SURVIVING);
            } else if (activeShips.Count == 0) {
                state = GameState.PostMatch;
                tieDelay.Start();

                foreach (var spaceship in spaceships) {
                    if (lastActiveShips.Contains(spaceship)) continue;
                    scoreBoard.AddScore(spaceship.Name, SCORE_FOR_TIE);
                }
            } else if (!suddenDeathTimer.IsActive) {
                if (!suddenDeathTimer.IsSet) return;
                DebugLog.Log(LogTag.HighPriority, "Sudden Death!");
                suddenDeathTimer.Clear();
                foreach (var spaceship in activeShips) {
                    SpaceshipKilled(spaceship);
                }
            } else {
                lastActiveShips = new List<Spaceship.Mutable>(activeShips);
            }
        }

        public static void SpawnShot(Spaceship.Mutable shooter) {
            instance.shotBodiesPool.Borrow<ShotBody>(shooter.Position, shooter.Rotation, instance.space, shooter);
            shooter.Body.ShotSound.Play();
        }

        public static void OnShieldUp() {
            instance.shieldUpSound.Play();
        }

        public static void RespawnedSpaceship(Spaceship.Mutable spaceship) {
            var go = instance.respawnEffectsPool.BorrowGameObject();
            var effect = go.GetComponent<IPoolable>();
            effect.Activate(spaceship.Position);
        }

        private void SpaceshipKilled(Spaceship.Mutable spaceship) {
            spaceship.BeDead();
            instance.explosionEffectsPool.Borrow<IPoolable>(spaceship.Position);
            suddenDeathTimer.Start();
        }
    }
}
