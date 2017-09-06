// todo move to VSS.VisionLink.Interfaces
using System;

namespace VSS.VisionLink.Interfaces.Events.TagFile
{
  public interface ITagFileEvent
  {
    Guid CustomerUID { get; set; }

    DateTime ActionUTC { get; set; }

    // todo do we need to keep this?
    DateTime ReceivedUTC { get; set; }
  }
}
