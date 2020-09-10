namespace CoreX.Wrapper
{
  public static class TGLLock
  {
    public static readonly object CsdManagementLock = new object();
    public static readonly object GeodeticXLock = new object();
  }
}
