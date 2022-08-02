using System;
using System.Collections;
using System.Collections.Generic;
using Avrahamy.Math;
using UnityEngine;


using StarWars.Actions;

namespace StarWars.Brains
{
    public class GirlsPowerBrain : SpaceshipBrain
    {
        public override string DefaultName
        {
            get { return "GirlsPower"; }
        }
        private Spaceship target = null;
        private Spaceship chaseTarget = null;
        private bool turnOnce;
        private const int BIGGER_DISTANCE = 15;
        private const int ANGLE_FOR_CHASE = 3;
        private const int LIMIT_DISTANCE = 4;
        private const int MAX_ENERGY = 3;
        
        public override IAction NextAction()
        {
            float distance = Mathf.Infinity;
            float chaseDistance =  Mathf.Infinity;
            float curDistance;
            
            if (!(target is {IsAlive: true}))
            {
                foreach (var ship in Space.Spaceships)
                {
                    // Find the closest ship for collision
                    if (spaceship != ship && ship.IsAlive)
                    {
                        curDistance = spaceship.ClosestRelativePosition(ship).magnitude;
                        if (curDistance < distance && curDistance < LIMIT_DISTANCE)
                        {
                            target = ship;
                            distance = curDistance;
                        }
                    }
                }
                
            }
            else
                target = null;
            
            
            if (!(chaseTarget is {IsAlive: true}))
            {
                foreach (var ship in Space.Spaceships)
                {
                    // Find the closest ship to chase
                    if (spaceship != ship && ship.IsAlive)
                    {
                        curDistance = spaceship.ClosestRelativePosition(ship).magnitude;
                        if (curDistance < chaseDistance)
                        {
                            chaseDistance = curDistance;
                            chaseTarget = ship;
                        }
                    }
                }
            }
            else
                chaseTarget = null;


            //raise shield if there is a spaceship very close to us
            if (target is {IsAlive: true})
            {
                if (target.IsShieldUp)
                    return TurnLeft.action;
                return spaceship.CanRaiseShield ? ShieldUp.action : spaceship.CanShoot ? Shoot.action:
                    TurnLeft.action;
            }
            
            //check if there is a shot near to the spaceship
            foreach (var shot in Space.Shots)
            {
                if (shot.IsAlive)
                {
                    Vector3 dis = spaceship.ClosestRelativePosition(shot);
                    var angle = dis.GetAngleBetweenXZ(shot.Forward);
                    curDistance = dis.magnitude;
                    if (curDistance < LIMIT_DISTANCE && 180 - Math.Abs(angle) < BIGGER_DISTANCE)
                    {
                        return spaceship.CanRaiseShield ? ShieldUp.action : DoNothing.action;
                    }
                }
            }
            
            //chase after the chase target
            if (chaseTarget is {IsAlive: true})
            {
                if (Space.Spaceships.Count == 2)
                {
                    if (!turnOnce)
                    {
                        turnOnce = true;
                        return TurnLeft.action;
                    }
                }
                var pos = spaceship.ClosestRelativePosition(chaseTarget);
                var forwardVector = spaceship.Forward;
                var angle = pos.GetAngleBetweenXZ(forwardVector);
                if (angle >= ANGLE_FOR_CHASE) return TurnLeft.action;
                if (angle <= -ANGLE_FOR_CHASE) return TurnRight.action;
                if (chaseDistance < BIGGER_DISTANCE && (!chaseTarget.IsShieldUp || chaseTarget.Energy < MAX_ENERGY)) 
                    return spaceship.CanShoot ? Shoot.action : DoNothing.action;
            }

            return DoNothing.action;

        }
        
    }
}
