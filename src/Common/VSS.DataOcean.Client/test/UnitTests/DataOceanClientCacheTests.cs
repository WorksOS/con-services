using System;
using Xunit;
using VSS.DataOcean.Client.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace VSS.DataOcean.Client.UnitTests
{
  public class DataOceanClientCacheTests
  {
    string _dataOceanRootFolderId = Guid.NewGuid().ToString();

    [Fact]
    public void DataOceanMemoryCache_NoCustomer()
    {
      var customerCache = new DataOceanFilePathCache(new MemoryCache(new MemoryCacheOptions()));

      var customerUid = Guid.NewGuid().ToString();
      var dataOceanCustomerFolderId = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var dataOceanProjectFolderId = Guid.NewGuid().ToString();

      var retrievedProjectDataOceanFolderId = customerCache.GetProjectDataOceanFolderId(customerUid, projectUid);
      retrievedProjectDataOceanFolderId.Should().BeNull();

      // do we have the customer?
      var customer = customerCache.GetCustomerFilePath(customerUid);
      if (customer == null)
      {
        _ = customerCache.GetOrCreateCustomerFilePath(customerUid, dataOceanCustomerFolderId);
        customerCache.GetOrCreateProject(customerUid, projectUid, dataOceanProjectFolderId);
      }

      retrievedProjectDataOceanFolderId = customerCache.GetProjectDataOceanFolderId(customerUid, projectUid);
      retrievedProjectDataOceanFolderId.Should().Be(dataOceanProjectFolderId);
    }

    [Fact]
    public void DataOceanMemoryCache_NoProject()
    {
      var customerCache = new DataOceanFilePathCache(new MemoryCache(new MemoryCacheOptions()));

      var customerUid = Guid.NewGuid().ToString();
      var dataOceanCustomerFolderId = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var dataOceanProjectFolderId = Guid.NewGuid().ToString();

      var retrievedProjectDataOceanFolderId = customerCache.GetProjectDataOceanFolderId(customerUid, projectUid);
      retrievedProjectDataOceanFolderId.Should().BeNull();

      var customer = customerCache.GetOrCreateCustomerFilePath(customerUid, dataOceanCustomerFolderId);

      // do we have the project?
      customer.Projects.TryGetValue(projectUid, out retrievedProjectDataOceanFolderId);
      if (retrievedProjectDataOceanFolderId == null)
        retrievedProjectDataOceanFolderId = customerCache.GetOrCreateProject(customerUid, projectUid, dataOceanProjectFolderId);

      retrievedProjectDataOceanFolderId.Should().Be(dataOceanProjectFolderId);
    }

    [Fact]
    public void DataOceanMemoryCache_AddDuplicateCustomer()
    {
      var customerCache = new DataOceanFilePathCache(new MemoryCache(new MemoryCacheOptions()));

      var customerUid = Guid.NewGuid().ToString();
      var dataOceanCustomerFolderId = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();

      var retrievedProjectDataOceanFolderId = customerCache.GetProjectDataOceanFolderId(customerUid, projectUid);
      retrievedProjectDataOceanFolderId.Should().BeNull();

      customerCache.GetOrCreateCustomerFilePath(customerUid, dataOceanCustomerFolderId);
      customerCache.GetOrCreateCustomerFilePath(customerUid, dataOceanCustomerFolderId);
      customerCache.GetCustomerFilePath(customerUid).DataOceanFolderId.Should().Be(dataOceanCustomerFolderId);
    }

    [Fact]
    public void DataOceanMemoryCache_AddDuplicateProject()
    {
      var customerCache = new DataOceanFilePathCache(new MemoryCache(new MemoryCacheOptions()));

      var customerUid = Guid.NewGuid().ToString();
      var dataOceanCustomerFolderId = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var dataOceanProjectFolderId = Guid.NewGuid().ToString();

      var retrievedProjectDataOceanFolderId = customerCache.GetProjectDataOceanFolderId(customerUid, projectUid);
      retrievedProjectDataOceanFolderId.Should().BeNull();

      var customer = customerCache.GetOrCreateCustomerFilePath(customerUid, dataOceanCustomerFolderId);

      // do we have the project?
      customer.Projects.TryGetValue(projectUid, out retrievedProjectDataOceanFolderId);
      if (retrievedProjectDataOceanFolderId == null)
        customerCache.GetOrCreateProject(customerUid, projectUid, dataOceanProjectFolderId);
      retrievedProjectDataOceanFolderId = customerCache.GetOrCreateProject(customerUid, projectUid, dataOceanProjectFolderId);
      customerCache.GetCustomerFilePath(customerUid).Projects.Count.Should().Be(1);

      retrievedProjectDataOceanFolderId.Should().Be(dataOceanProjectFolderId);
    }

    [Fact]
    public void DataOceanMemoryCache_MultipleProjects()
    {
      var customerCache = new DataOceanFilePathCache(new MemoryCache(new MemoryCacheOptions()));

      var customerUid = Guid.NewGuid().ToString();
      var dataOceanCustomerFolderId = Guid.NewGuid().ToString();
      var projectUid1 = Guid.NewGuid().ToString();
      var dataOceanProjectFolderId1 = Guid.NewGuid().ToString();
      var projectUid2 = Guid.NewGuid().ToString();
      var dataOceanProjectFolderId2 = Guid.NewGuid().ToString();

      _ = customerCache.GetOrCreateCustomerFilePath(customerUid, dataOceanCustomerFolderId);
      _ = customerCache.GetOrCreateProject(customerUid, projectUid1, dataOceanProjectFolderId1);
      _ = customerCache.GetOrCreateProject(customerUid, projectUid2, dataOceanProjectFolderId2);

      customerCache.GetCustomerFilePath(customerUid).Projects.Count.Should().Be(2);
      customerCache.GetProjectDataOceanFolderId(customerUid, projectUid1).Should().Be(dataOceanProjectFolderId1);
      customerCache.GetProjectDataOceanFolderId(customerUid, projectUid2).Should().Be(dataOceanProjectFolderId2);
    }
  }
}
