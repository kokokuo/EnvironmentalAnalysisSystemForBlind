using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraCrossingDetector
{
    public class IntensityPoint
    {
        private PointF location;
        private double intensity;

        public IntensityPoint()
        {
            location = new PointF();
            intensity = -1;
        }

        public bool IsEmpty()
        {
            if (location.IsEmpty && intensity == -1)
                return true;
            return false;
        }
        public void SetData(PointF p, double value)
        {
            location = p;
            intensity = value;
        }
        public PointF GetLocation() { return location; }
        public double GetIntensity() { return intensity; }
    }
}
