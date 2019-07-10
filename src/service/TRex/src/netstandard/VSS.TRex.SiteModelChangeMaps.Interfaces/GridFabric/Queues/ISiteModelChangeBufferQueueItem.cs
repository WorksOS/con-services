using System;

namespace VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Queues
{
  public interface ISiteModelChangeBufferQueueItem
  {
    /// <summary>
    /// The date at which the item was inserted into the buffer queue. This field is indexed to permit
    /// processing items in the order they arrived
    /// </summary>
    DateTime InsertUTC { get; set; }

    /// <summary>
    /// The contents of the site model change, as a byte array
    /// </summary>
    byte[] Content { get; set; }

    /// <summary>
    /// UID identifier of the project this change map relates to
    /// This field is used as the affinity key map that determines which mutable server will
    /// store this TAG file.
    /// </summary>
    Guid ProjectUID { get; set; }

    /// <summary>
    /// The type of operation to be performed between the change map content in this item and the
    /// destination change map maintained for a machine in a project
    /// </summary>
    SiteModelChangeMapOperation Operation { get; set; }

    /// <summary>
    /// The origin of the change map delta represented by this item, such as production data ingest
    /// or query processing 
    /// </summary>
    SiteModelChangeMapOrigin Origin { get; set; }
  }
}
