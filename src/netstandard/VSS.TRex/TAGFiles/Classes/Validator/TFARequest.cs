using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    public class TFARequest
    { 
        public string projectUid;
        public int deviceType;
        public string radioSerial;
        public string tccOrgUid;
        public double latitude;
        public double longitude;
        public DateTime timeOfPosition;
    }
}
