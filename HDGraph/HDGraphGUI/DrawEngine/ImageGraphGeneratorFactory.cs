﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HDGraph.DrawEngine
{
    public enum DrawType
    {
        Circular,
        Rectangular,
    }

    public class ImageGraphGeneratorFactory
    {
        public static ImageGraphGeneratorBase CreateGenerator(DrawType type, DirectoryNode node, HDGraphScanEngine engine)
        {
            switch (type)
            {
                case DrawType.Circular:
                    return new CircularImageGraphGenerator(node, engine);
                case DrawType.Rectangular:
                    return new RectangularImageGraphGenerator(node, engine);
                default:
                    throw new NotSupportedException();
                    break;
            }
        }
    }
}
