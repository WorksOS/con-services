using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests
{
  [TestClass]
  public class SummaryHelperTests
  {
    [TestMethod]
    public void TestSummaryHelper_OneServiceView()
    {
      var serviceView = GetServiceView(1, "Essentials", 20150603, 20170603, 1, "ABC12345", 1, "Customer One");
      var serviceViews = new List<ServiceViewInfoDto> {serviceView};
      var summary = SummaryHelper.GetServiceViewSummary(serviceViews);
      Assert.AreEqual(
        "ID: 1 Type: Essentials Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer One." +
        Environment.NewLine, summary);
    }

    [TestMethod]
    public void TestSummaryHelper_MultipleServiceViews()
    {
      var serviceView1 = GetServiceView(1, "Essentials", 20150603, 20170603, 1, "ABC12345", 1, "Customer One");
      var serviceView2 = GetServiceView(2, "Health", 20150603, 20170603, 1, "ABC12345", 1, "Customer One");
      var serviceView3 = GetServiceView(3, "Maintenance", 20150603, 20170603, 1, "ABC12345", 1, "Customer One");
      var serviceViews = new List<ServiceViewInfoDto> {serviceView1, serviceView2, serviceView3};
      var summary = SummaryHelper.GetServiceViewSummary(serviceViews);
      Assert.AreEqual(
        "ID: 1 Type: Essentials Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer One." +
        Environment.NewLine +
        "ID: 2 Type: Health Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer One." +
        Environment.NewLine +
        "ID: 3 Type: Maintenance Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer One." +
        Environment.NewLine, summary);
    }

    [TestMethod]
    public void TestSummaryHelper_CustomerNameHasCurlyBracesInIt()
    {
      var serviceView = GetServiceView(1, "Essentials", 20150603, 20170603, 1, "ABC12345", 1, "Customer {One} {Two}");
      var serviceViews = new List<ServiceViewInfoDto> { serviceView };
      var summary = SummaryHelper.GetServiceViewSummary(serviceViews);
      Assert.AreEqual(
        "ID: 1 Type: Essentials Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer {{One}} {{Two}}." +
        Environment.NewLine, summary);
    }

    [TestMethod]
    public void TestSummaryHelper_CustomerNameHasOpenCurlyBraceInIt()
    {
      var serviceView = GetServiceView(1, "Essentials", 20150603, 20170603, 1, "ABC12345", 1, "Customer One{");
      var serviceViews = new List<ServiceViewInfoDto> { serviceView };
      var summary = SummaryHelper.GetServiceViewSummary(serviceViews);
      Assert.AreEqual(
        "ID: 1 Type: Essentials Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer One{{." +
        Environment.NewLine, summary);
    }

    [TestMethod]
    public void TestSummaryHelper_CustomerNameHasClosedCurlyBraceInIt()
    {
      var serviceView = GetServiceView(1, "Essentials", 20150603, 20170603, 1, "ABC12345", 1, "Customer One}");
      var serviceViews = new List<ServiceViewInfoDto> { serviceView };
      var summary = SummaryHelper.GetServiceViewSummary(serviceViews);
      Assert.AreEqual(
        "ID: 1 Type: Essentials Active: 20150603 to 20170603 for AssetID: 1 SN: ABC12345 for Customer ID: 1 Name: Customer One}}." +
        Environment.NewLine, summary);
    }

    private static ServiceViewInfoDto GetServiceView(long serviceViewId, string serviceTypeName, int startDateKey,
      int endDateKey, long assetId, string assetSerialNumber, long customerId, string customerName)
    {
      return new ServiceViewInfoDto
      {
        ServiceViewId = serviceViewId,
        ServiceTypeName = serviceTypeName,
        StartDateKey = startDateKey,
        EndDateKey = endDateKey,
        AssetId = assetId,
        AssetSerialNumber = assetSerialNumber,
        CustomerId = customerId,
        CustomerName = customerName
      };
    }
  }
}