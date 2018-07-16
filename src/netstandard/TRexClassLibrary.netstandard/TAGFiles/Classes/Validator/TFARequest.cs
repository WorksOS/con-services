using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    public class TFARequest
    {
        public string radioSerial;
        public int deviceType;
        public double latitude;
        public double longitude;
        public DateTime timeOfPosition;
        public Guid tccOrgUid;
        public Guid? projectUid;
    }
}
