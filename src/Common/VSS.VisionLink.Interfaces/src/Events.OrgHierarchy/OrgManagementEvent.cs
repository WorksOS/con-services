using VSS.VisionLink.Interfaces.Events.OrgHierarchy.Operations;

namespace VSS.VisionLink.Interfaces.Events.OrgHierarchy
{
  public class OrgManagementEvent
  {
    // Org Management
    public OrgHierarchyRootCreatedEvent OrgHierarchyRootCreatedEvent { get; set; }
    public OrgAssociatedEvent OrgAssociatedEvent { get; set; }
    public OrgDissociatedEvent OrgDissociatedEvent { get; set; }
    public OrgRemovedByCustomerUidEvent OrgRemovedByCustomerUidEvent { get; set; }
    public OrgRemovedByOrgUidEvent OrgRemovedByOrgUidEvent { get; set; }

    // Org Asset Management
    public OrgAssetsAssociatedEvent OrgAssetsAssociatedEvent { get; set; }
    public OrgAssetsDissociatedEvent OrgAssetsDissociatedEvent { get; set; }
  }
}
