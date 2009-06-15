﻿using System;
using System.Collections.Generic;
using System.Text;
using HDGraph.Interfaces.ScanEngines;

namespace HDGraph.DrawEngine
{
    public enum DrawType
    {
        Circular,
        Rectangular,
    }

    public class ImageGraphGeneratorFactory
    {
        public static ImageGraphGeneratorBase CreateGenerator(DrawType type, IDirectoryNode node, HDGraphScanEngineBase engine)
        {
            switch (type)
            {
                case DrawType.Circular:
                    return new CircularImageGraphGenerator(node, engine);
                case DrawType.Rectangular:
                    return new RectangularImageGraphGenerator(node, engine);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
