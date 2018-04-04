﻿using System;
using CC = System.Drawing;
using VSS.TRex.Rendering.Abstractions;

namespace VSS.TRex.Rendering.Implementations.Core2
{
  public class Bitmap : IBitmap, IDisposable
  {

    private readonly CC.Bitmap container;

    internal CC.Bitmap UnderlyingBitmap => container;

    internal Bitmap(int x, int y)
    {
      container = new CC.Bitmap(x, y);
    }

    public int Width => container.Width;
    public int Height => container.Height;

    public object GetBitmap() => UnderlyingBitmap;

    public void Dispose()
    {
      container?.Dispose();
    }
  }

}
