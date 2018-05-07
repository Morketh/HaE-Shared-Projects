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
        public class LCDDrawingLib
	    {
            private Canvas mainCanvas;
            private HashSet<IMonoElement> elements = new HashSet<IMonoElement>();

            public Color backgroundColor;

            public LCDDrawingLib(int sizeX, int sizeY, Color background)
            {
                backgroundColor = background;

                mainCanvas = new Canvas(sizeX, sizeY);
            }

            public void AddElement(IMonoElement element)
            {
                elements.Add(element);
            }

            public StringBuilder Draw()
            {
                mainCanvas.SetBackGround(backgroundColor);

                foreach (var element in elements)
                {
                    mainCanvas.MergeCanvas(element.Draw(), element.Position);
                }

                return mainCanvas.ToStringBuilder();
            }
        }
	}
}
