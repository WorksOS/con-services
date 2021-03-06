﻿namespace VSS.Productivity3D.Common
{
  public class VelociraptorConstants
  {
    public const int NO_PROJECT_ID = -1;
    public const int NO_MACHINE_ID = -1;
    public const double NO_HEIGHT = 1E9;
    public const short NO_CCV = short.MaxValue;
    public const short NO_MDP = short.MaxValue;
    public const ushort NO_TEMPERATURE = 4096;
    public const ushort NO_PASSCOUNT = ushort.MinValue;
    public const float NULL_SINGLE = -3.4E38F; 
    public const ushort NO_SPEED = ushort.MaxValue;
    public const double VOLUME_CHANGE_TOLERANCE = 0.0;

    // DXF linework processing
    public const int MAX_BOUNDARIES_TO_PROCESS = 500;
    public const int MAX_VERTICES_PER_BOUNDARY = 50;
  }
}
