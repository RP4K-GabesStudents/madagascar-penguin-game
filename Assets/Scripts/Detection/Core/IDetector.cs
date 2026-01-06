using System;

namespace Detection.Core
{
    public interface IDetector
    {
        public int UpdateDetector(DetectedObject [] detectedObjects);
        
        

    }
}
