using System;
using System.Collections;
using System.Collections.Generic;
using Avrahamy.Math;
using UnityEngine;

using StarWars.Actions;
using Random = UnityEngine.Random;

namespace StarWars.Brains
{
    public class CrazyBrain : SpaceshipBrain
    {
        private static float countTurn = 0;
        private float curDistance = 0;
        private int limitDistance = 3;

        public override string DefaultName
        {
            get { return "CrazyShip"; }
        }


        public override IAction NextAction()
        {
            countTurn += Time.deltaTime;
            foreach (var shot in Space.Shots)
            {
                if (shot.IsAlive)
                {
                    Vector3 dis = spaceship.ClosestRelativePosition(shot);
                    var angle = dis.GetAngleBetweenXZ(shot.Forward);
                    curDistance = dis.magnitude;
                    if (curDistance < limitDistance && 180 - Math.Abs(angle) < 15)
                    {
                        return spaceship.CanRaiseShield ? ShieldUp.action : DoNothing.action;
                    }
                }
            }
            
            if (countTurn >= 0 && countTurn <= 0.5)
                return TurnRight.action;

            if (countTurn >= 1 && countTurn <= 1.5)
                return TurnLeft.action;
            
            if (countTurn > 1.5)
            {
                countTurn = 0;
                return spaceship.CanRaiseShield ? Shoot.action : spaceship.CanShoot ? Shoot.action : DoNothing.action;
            }
            
            return spaceship.CanShoot ? Shoot.action : DoNothing.action;
        }

    }

}