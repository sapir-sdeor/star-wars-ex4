using UnityEngine;
using Avrahamy;
using Avrahamy.Math;
using Avrahamy.Utils;

namespace StarWars {
    /// <summary>
    /// A visible body. Can move and rotate.
    /// </summary>
    public abstract class Body : OptimizedBehaviour {

        public float Rotation {
            get {
                return transform.eulerAngles.y;
            }
            set {
                transform.SetYRotation(value);
            }
        }

        public Vector3 Position {
            get {
                return transform.position;
            }
            set {
                transform.position = value;
            }
        }

        public Vector3 Forward {
            get {
                return Vector3.right.Rotate(Rotation, Vector3.up);
            }
            set {
                transform.position = value;
            }
        }

        public void FixPosition() {
            var pos = Position;
            var bounds = Game.Size / 2;
            var didTeleport = false;
            if (pos.x < -bounds.x) {
                pos.x += bounds.x * 2;
                didTeleport = true;
            }
            if (pos.x >= bounds.x) {
                pos.x -= bounds.x * 2;
                didTeleport = true;
            }
            if (pos.z < -bounds.y) {
                pos.z += bounds.y * 2;
                didTeleport = true;
            }
            if (pos.z >= bounds.y) {
                pos.z -= bounds.y * 2;
                didTeleport = true;
            }
            Position = pos;
            if (didTeleport) {
                OnTeleported();
            }
        }

        public void MoveForward(float speed) {
            var direction = Forward.GetWithMagnitude(speed);
            DebugDraw.DrawLine(Position, Position + Forward, Color.red);
            Position += direction;
            FixPosition();
        }

        protected virtual void OnTeleported() {
            // Do nothing.
        }
    }
}
