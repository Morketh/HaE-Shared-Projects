﻿using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
	partial class Program
	{
        public class AdvThrustControl
	    {
            private IMyShipController controller;
            private IngameTime ingameTime;

            private Dictionary<Vector3D, List<IMyThrust>> sortedThrusters = new Dictionary<Vector3D, List<IMyThrust>>();

            private Dictionary<Vector3D, int> thrustersPerDirection = new Dictionary<Vector3D, int>();

            private Dictionary<Vector3D, ThrusterSide> thrustersInDirection = new Dictionary<Vector3D, ThrusterSide>();

            public AdvThrustControl(IMyShipController controller, List<IMyThrust> thrusters,IngameTime ingameTime, PID_Controller.PIDSettings PidSettings)
            {
                this.controller = controller;
                this.ingameTime = ingameTime;

                SortThrustersByDirection(thrusters, controller, PidSettings);
                CountThrustersPerDirection();
            }

            public void CountThrustersPerDirection()
            {
                foreach (var thrustInDir in sortedThrusters.Keys)
                {
                    var thrusterList = sortedThrusters[thrustInDir];

                    thrustersPerDirection[thrustInDir] = thrusterList.Count;
                }
            }

            public void SortThrustersByDirection(List<IMyThrust> thrusters, IMyShipController reference, PID_Controller.PIDSettings pidSettings)
            {
                var sortedThrusters = new Dictionary<Vector3D, List<IMyThrust>>();

                foreach (var thruster in thrusters)
                {
                    var relativeThrustVector = VectorUtils.TransformDirWorldToLocal(reference.WorldMatrix, thruster.WorldMatrix.Backward);

                    if (!sortedThrusters.ContainsKey(relativeThrustVector))
                        sortedThrusters[relativeThrustVector] = new List<IMyThrust>();

                    if (!thrustersInDirection.ContainsKey(relativeThrustVector))
                    {
                        ThrusterSide side = new ThrusterSide
                        {
                            thrustDirection = relativeThrustVector,
                            pid = new PID_Controller(pidSettings),
                            ingameTime = this.ingameTime,
                            thrusters = new HashSet<IMyThrust>()
                        };

                        thrustersInDirection[relativeThrustVector] = side;
                    }

                    thrustersInDirection[relativeThrustVector].thrusters.Add(thruster);
                    sortedThrusters[relativeThrustVector].Add(thruster);
                }
            }

            public void ThrustToVelocity(Vector3D velocity)
            {
                Vector3D accel = velocity - controller.GetShipVelocities().LinearVelocity;
                double Magnitude = accel.Normalize();

                Vector3D localAccel = VectorUtils.TransformDirWorldToLocal(controller.WorldMatrix, accel);
                localAccel *= Magnitude;

                foreach (var thrustSide in thrustersInDirection.Values)
                {
                    thrustSide.ProjectThrustOnDirection(localAccel);
                }
            }


            private class ThrusterSide
            {
                public Vector3D thrustDirection;
                public PID_Controller pid;
                public IngameTime ingameTime;

                public int Amount => thrusters.Count;
                public HashSet<IMyThrust> thrusters;

                private TimeSpan lastTime;
                private float currentThrustAmount;

                public void ProjectThrustOnDirection(Vector3D localDesiredAcceleration)
                {
                    double thrustAxisProjection = VectorUtils.GetProjectionScalar(localDesiredAcceleration, thrustDirection);

                    double timeSinceLastRun = lastTime.TotalSeconds;
                    lastTime = ingameTime.Time;

                    float thrustAmount = (float)pid.NextValue(thrustAxisProjection, timeSinceLastRun);


                    if (thrustAmount > 0 && thrustAmount != currentThrustAmount)
                    {
                        ThrustUtils.SetThrustPercentage(thrusters, thrustAmount);
                        currentThrustAmount = thrustAmount;
                    }   
                    else if (thrustAmount <= 0 && currentThrustAmount != 0)
                    {
                        ThrustUtils.SetThrustPercentage(thrusters, 0);
                        currentThrustAmount = 0;
                    }
                }
            }
        }
	}
}