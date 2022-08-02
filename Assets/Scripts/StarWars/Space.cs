using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Avrahamy;
using Avrahamy.Collections;
using Avrahamy.Math;
using StarWars.Brains;
using MIConvexHull;

namespace StarWars {
    /// <summary>
    /// Read only space. Can't be changed.
    /// Holds all the spaceships and shots.
    /// </summary>
    public static class Space {

        private static readonly ObjectPool<Spaceship> spaceshipsPool =
            new ObjectPool<Spaceship>(() => new Spaceship(), 3, 1);
        private static readonly ObjectPool<Spaceship.Mutable> spaceshipMutablesPool =
            new ObjectPool<Spaceship.Mutable>(() => new Spaceship.Mutable(), 3, 1);
        private static readonly ObjectPool<Shot> shotsPool =
            new ObjectPool<Shot>(() => new Shot(), 10, 5);
        private static readonly ObjectPool<Shot.Mutable> shotMutablesPool =
            new ObjectPool<Shot.Mutable>(() => new Shot.Mutable(), 10, 5);

        private static readonly List<Spaceship> spaceships = new List<Spaceship>();
        private static readonly List<Spaceship.Mutable> spaceshipMutables = new List<Spaceship.Mutable>();
        private static readonly List<Shot> shots = new List<Shot>();
        private static readonly List<Shot.Mutable> shotMutables = new List<Shot.Mutable>();

        public static IList<Spaceship> Spaceships {
            get {
                return spaceships;
            }
        }

        public static IList<Shot> Shots {
            get {
                return shots;
            }
        }

        public static Vector3 GetSpawnPoint() {
            var vertices = new Vertex2[4 + spaceships.Count + shots.Count];
            var index = 0;
            var spaceHalfSize = Game.Size / 2;
            vertices[index++] = new Vertex2(-spaceHalfSize.x, -spaceHalfSize.y);
            vertices[index++] = new Vertex2(spaceHalfSize.x, -spaceHalfSize.y);
            vertices[index++] = new Vertex2(-spaceHalfSize.x, spaceHalfSize.y);
            vertices[index++] = new Vertex2(spaceHalfSize.x, spaceHalfSize.y);
            foreach (var obj in spaceships) {
                vertices[index++] = new Vertex2(obj.Position.x, obj.Position.z);
            }
            foreach (var obj in shots) {
                vertices[index++] = new Vertex2(obj.Position.x, obj.Position.z);
            }
            var triangulation = Triangulation.CreateDelaunay<Vertex2, Cell2>(vertices);
            var maxTriangle = triangulation.Cells.First();
            float maxArea = 0;
            foreach (var triangle in triangulation.Cells) {
                var area = triangle.Area;
                if (area > maxArea) {
                    maxArea = area;
                    maxTriangle = triangle;
                }
            }
            return maxTriangle.Centroid.ToVector3XZ();
        }

        public static void RespawnedSpaceship(Spaceship.Mutable spaceship) {
            spaceships.Add(spaceship.obj);
        }

        /// <summary>
        /// Allows mutating the space.
        /// Doesn't inherit from Space to not allow casting the Space to Space.Mutable.
        /// </summary>
        public class Mutable {
            public IList<Spaceship.Mutable> Spaceships {
                get {
                    return spaceshipMutables;
                }
            }

            public IList<Shot.Mutable> Shots {
                get {
                    return shotMutables;
                }
            }

            public void RegisterSpaceship(SpaceshipBody body, SpaceshipBrain brain) {
                var spaceship = spaceshipsPool.Borrow();
                var spaceshipMutable = spaceshipMutablesPool.Borrow();
                spaceshipMutable.Activate(spaceship, body, brain, Game.LivesPerShip);
                DebugLog.Log("RegisterSpaceship " + spaceship.Name);

                spaceships.Add(spaceship);
                spaceshipMutables.Add(spaceshipMutable);
            }

            public void RegisterShot(ShotBody body, Spaceship.Mutable shooter) {
                var shot = shotsPool.Borrow();
                var shotMutable = shotMutablesPool.Borrow();
                shotMutable.Activate(shot, body, shooter);

                shots.Add(shot);
                shotMutables.Add(shotMutable);
            }

            /// <summary>
            /// Only removes the spaceship from the list of active spaceships.
            /// </summary>
            public void RemoveSpaceship(Spaceship.Mutable spaceship) {
                spaceships.Remove(spaceship.obj);
            }

            /// <summary>
            /// Completely removes a shot.
            /// </summary>
            public void RemoveShot(Shot.Mutable shot) {
                shots.Remove(shot.obj);
                shotMutables.Remove(shot);
                shotsPool.Return(shot.obj);
                shotMutablesPool.Return(shot);
                shot.Deactivate();
            }

            public void ClearSpace() {
                foreach (var item in spaceshipMutables) {
                    item.Deactivate();
                }
                spaceshipsPool.ReturnAll();
                spaceshipMutablesPool.ReturnAll();

                spaceships.Clear();
                spaceshipMutables.Clear();

                shots.Clear();
                shotMutables.Clear();
                shotsPool.ReturnAll();
                shotMutablesPool.ReturnAll();
            }
        }
    }
}
