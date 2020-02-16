using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetDeviceContextTests
  {
    protected AssetDeviceContext Context;

    [TestInitialize]
    public void AssetDeviceContextTests_Init()
    {
      Context = new AssetDeviceContext();
    }

    /* VALID DEVICE OWNER NOTE
		 * Only Dealers and Accounts can "own" devices.
     * This aids in validation.
		 */

    #region IsValidDeviceOwner
    [TestMethod]
    public void IsValidDeviceOwner_OwnerIsDealer_True()
    {
      Context.Owner.Type = CustomerTypeEnum.Dealer;

      Assert.IsTrue(Context.IsValidDeviceOwner(), "Should be true");
    }

    [TestMethod]
    public void IsValidDeviceOwner_OwnerIsAccount_True()
    {
      Context.Owner.Type = CustomerTypeEnum.Account;

      Assert.IsTrue(Context.IsValidDeviceOwner(), "Should be true");
    }

    [TestMethod]
    public void IsValidDeviceOwner_OwnerIsCustomer_False()
    {
      Context.Owner.Type = CustomerTypeEnum.Customer;

      Assert.IsFalse(Context.IsValidDeviceOwner(), "Should be false");
    }
    #endregion

    /* DEVICE BEING INSTALLED NOTE
		 * IsDeviceBeingInstalled let's now that 
     * VL's state is dictating that Asset/Device 
     * relationships are changing.
		 */

    #region IsDeviceBeingInstalled
    [TestMethod]
    public void IsDeviceBeingInstalled_DeviceDoesNotExist_True()
    {
      Context.Device.Id = 0;

      Assert.IsTrue(Context.IsDeviceBeingInstalled());
    }

    [TestMethod]
    public void IsDeviceBeingInstalled_DeviceExists_AssetDoesNotExist_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = 0;

      Assert.IsTrue(Context.IsDeviceBeingInstalled());
    }

    [TestMethod]
    public void IsDeviceBeingInstalled_DeviceAndAssetExist_DeviceNotInstalledOnAsset_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.AssetId = 0;
      Context.Asset.AssetId = IdGen.GetId();

      Assert.IsTrue(Context.IsDeviceBeingInstalled());
    }

    [TestMethod]
    public void IsDeviceBeingInstalled_DeviceAndAssetExist_DeviceInstalled_AssetHasNoDevice_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.AssetId = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = 0;

      Assert.IsTrue(Context.IsDeviceBeingInstalled());
    }

    [TestMethod]
    public void IsDeviceBeingInstalled_DeviceAndAssetExist_DeviceInstalled_AssetHasDifferentDevice_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.AssetId = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();

      Assert.IsTrue(Context.IsDeviceBeingInstalled());
    }

    [TestMethod]
    public void IsDeviceBeingInstalled_DeviceAndAssetExist_DeviceInstalled_AssetHasSameDeviceInstalled_False()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Device.AssetId = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = Context.Device.Id;

      Assert.IsTrue(Context.IsDeviceBeingInstalled());
    }
    #endregion

    /* IMPORTANT DEVICE TRANSFER NOTE
		 * A Device Transfer occurs when we are moving an
     * EXISTING Device to a different Asset and it is
     * NOT a Device Replacement.
     * 
     * It will be invalid to execute a Device Transfer
     * when either the IB Device is active or the Asset's
     * currently installed Device (if it exists) is
     * active, but that does not define a Device Transfer
     * so we will catch that invalid scenario in the
     * validations and not in the IsDeviceTransfer
     * method implementation.
		 */

    #region IsDeviceTransfer

    [TestMethod]
    public void IsDeviceTransfer_DeviceExists_AssetHasNoDeviceInstalled_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = 0;

      Assert.IsTrue(Context.IsDeviceTransfer());
    }

    [TestMethod]
    public void IsDeviceTransfer_DeviceExists_AssetHasDifferentDeviceInstalled_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();

      Assert.IsTrue(Context.IsDeviceTransfer());
    }

    [TestMethod]
    public void IsDeviceTransfer_DeviceDoesNotExist_False()
    {
      Context.Device.Id = 0;
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();

      Assert.IsFalse(Context.IsDeviceTransfer());
    }

    #endregion

    /* IMPORTANT DEVICE REPLACEMENT NOTE
     * 
		 * A Device Replacement occurs when the IB Device
     * is being moved onto an Asset that currently has
     * a different active Device installed.
     * 
     * It is invalid for the IB Device to be active,
     * but that does not define a Device Replacement
     * so we will catch that invalid scenario in the
     * validations and not in the IsDeviceReplacement
     * method implementation.
		 */

    #region IsDeviceReplacement

    [TestMethod]
    public void IsDeviceReplacement_DeviceDoesNotExist_AssetHasActiveDeviceInstalled_True()
    {
      Context.Device.Id = 0;
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      Assert.IsTrue(Context.IsDeviceReplacement());
    }

    [TestMethod]
    public void IsDeviceReplacement_DeviceExists_AssetHasActiveDeviceInstalled_True()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Subscribed;

      Assert.IsTrue(Context.IsDeviceReplacement());
    }

    [TestMethod]
    public void IsDeviceReplacement_DeviceDoesNotExist_AssetDoesNotExist_False()
    {
      Context.Device.Id = 0;
      Context.Asset.AssetId = 0;

      Assert.IsFalse(Context.IsDeviceReplacement());
    }

    [TestMethod]
    public void IsDeviceReplacement_DeviceExists_AssetDoesNotExist_False()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = 0;

      Assert.IsFalse(Context.IsDeviceReplacement());
    }

    [TestMethod]
    public void IsDeviceReplacement_DeviceExists_AssetHasNoDeviceInstalled_False()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = 0;

      Assert.IsFalse(Context.IsDeviceReplacement());
    }

    [TestMethod]
    public void IsDeviceReplacement_DeviceExists_AssetHasInactiveDeviceInstalled_False()
    {
      Context.Device.Id = IdGen.GetId();
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.Device.DeviceState = DeviceStateEnum.Provisioned;

      Assert.IsFalse(Context.IsDeviceReplacement());
    }

    #endregion

    /* IMPORTANT OWNERSHIP TRANSFER NOTE
     * 
		 * It is only considered an Ownership Transfer if the 
     * Context.Device's Owner is different from the Context.Owner
     * It is not an Ownership Transfer when the Context.Asset's 
     * Device has a different Owner.
		 */

    #region IsOwnershipTransfer

    [TestMethod]
    public void IsOwnershipTransfer_DeviceExistsWithDifferentOwner_True()
    {
      Context.Owner.Id = IdGen.GetId();
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerId = IdGen.GetId();

      Assert.IsTrue(Context.IsOwnershipTransfer());
    }

    [TestMethod]
    public void IsOwnershipTransfer_DeviceExistsWithInactiveOwner_True()
    {
      Context.Owner.Id = IdGen.GetId();
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerId = 0;

      Assert.IsTrue(Context.IsOwnershipTransfer());
    }

    [TestMethod]
    public void IsOwnershipTransfer_DeviceDoesNotExist_False()
    {
      Context.Owner.Id = IdGen.GetId();
      Context.Device.Id = 0;

      Assert.IsFalse(Context.IsOwnershipTransfer());
    }

    [TestMethod]
    public void IsOwnershipTransfer_DeviceExistsWithSameOwner_False()
    {
      Context.Owner.Id = IdGen.GetId();
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerId = Context.Owner.Id;

      Assert.IsFalse(Context.IsOwnershipTransfer());
    }

    /// <summary>
    /// It is only considered an Ownership Transfer if the 
    /// Context.Device's Owner is different from the Context.Owner
    /// It is not an Ownership Transfer when the Context.Asset's 
    /// Device has a different Owner.
    /// </summary>
    [TestMethod]
    public void IsOwnershipTransfer_DeviceExistsWithSameOwner_AssetHasDeviceInstalledWithDifferentOwner_False()
    {
      Context.Owner.Id = IdGen.GetId();
      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerId = Context.Owner.Id;
      Context.Asset.AssetId = IdGen.GetId();
      Context.Asset.DeviceId = IdGen.GetId();
      Context.Asset.DeviceOwnerId = IdGen.GetId();

      Assert.IsFalse(Context.IsOwnershipTransfer());
    }

    #endregion
  }
}
