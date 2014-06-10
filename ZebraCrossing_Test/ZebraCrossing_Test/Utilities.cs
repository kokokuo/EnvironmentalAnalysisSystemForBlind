using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraCrossingDetector
{
    public static class Utilities
    {
        public static Bgr[] LineColors = new Bgr[]{
                new Bgr(255,0,0),
                new Bgr(255,255,0),
                new Bgr(0,255,0),
                new Bgr(0,255,255),
                new Bgr(0,0,255),
                new Bgr(255,0,255),
                new Bgr(255,0,128),
                new Bgr(128,0,128),
                new Bgr(128,0,255),
                new Bgr(128,255,255),
                new Bgr(128,128,255),
                new Bgr(255,128,128),
                new Bgr(255,128,0),
                new Bgr(255,128,0),
            };
    }
}
