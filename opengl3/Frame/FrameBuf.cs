using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class FrameBuf
    {
        public Frame[] laserFrames;
        public Frame origFrame;
        public FrameBuf(Frame[] _laserFrames, Frame _origFrame)
        {
            laserFrames = _laserFrames;
            origFrame = _origFrame;
        }

        public Frame[] getDiffFrames()
        {
            var dif_frs = (Frame[])laserFrames.Clone();
            for(int i=0; i<dif_frs.Length;i++)
            {
                dif_frs[i].im -= origFrame.im;
            }
            return dif_frs;
        }
    }
}
