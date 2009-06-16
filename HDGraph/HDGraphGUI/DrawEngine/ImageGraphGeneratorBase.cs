﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using HDGraph.Interfaces.ScanEngines;

namespace HDGraph.DrawEngine
{
    public abstract class ImageGraphGeneratorBase
    {
        public abstract BiResult<Bitmap, InternalDrawOptions> Draw(bool drawImage, bool drawText, InternalDrawOptions options);

        public abstract IDirectoryNode FindNodeByCursorPosition(Point curseurPos);
    }
}
