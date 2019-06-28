using VSS.Hydrology.WebApi.DXF.Collections;
using VSS.Hydrology.WebApi.DXF.Tables;

namespace VSS.Hydrology.WebApi.DXF
{
    public delegate void XDataAddAppRegEventHandler(IHasXData sender, ObservableCollectionEventArgs<ApplicationRegistry> e);
    public delegate void XDataRemoveAppRegEventHandler(IHasXData sender, ObservableCollectionEventArgs<ApplicationRegistry> e);

    /// <summary>
    /// Supports <see cref="DxfObject">DxfObjects</see> that contain extended data information.
    /// </summary>
    public interface IHasXData
    {
        event XDataAddAppRegEventHandler XDataAddAppReg;
        event XDataRemoveAppRegEventHandler XDataRemoveAppReg;

        /// <summary>
        /// Gets the object <see cref="XDataDictionary">extended data</see>.
        /// </summary>
        XDataDictionary XData { get; }
    }
}
