using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSP.MasterData.Asset.Data.Helpers;
using VSS.MasterData.Asset.AcceptanceTests.Models;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService;

namespace VSS.MasterData.Asset.AcceptanceTests.TestData
{
  class TestDataGenerator
  {
    //DB Configuration

    public string MySqlDBName = AssetServiceConfig.MySqlDBName;



    public int CreateAsset(string _connectionString, CreateAssetEvent asset)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into Asset ( AssetUID, AssetName, LegacyAssetID, SerialNumber, MakeCode, Model, AssetTypeName,IconKey,EquipmentVIN,ModelYear,InsertUTC,UpdateUTC,StatusInd) values ({0}, @AssetName, @LegacyAssetID, @SerialNumber, @MakeCode, @Model, @AssetTypeName,@IconKey,@EquipmentVIN,@ModelYear,@InsertUTC,@UpdateUTC,@StatusInd);"
                      , asset.AssetUID.ToStringWithoutHyphens().WrapWithUnhex()
                      ),
                      new
                      {
                        AssetName = asset.AssetName == "$#$#$" || String.IsNullOrWhiteSpace(asset.AssetName) ? null : asset.AssetName,
                        LegacyAssetID = asset.LegacyAssetID == -9999999 ? 0 : asset.LegacyAssetID,
                        SerialNumber = asset.SerialNumber,
                        MakeCode = asset.MakeCode,
                        Model = asset.Model == "$#$#$" || String.IsNullOrWhiteSpace(asset.Model) ? null : asset.Model,
                        AssetTypeName = asset.AssetType == "$#$#$" || String.IsNullOrWhiteSpace(asset.AssetType) ? null : asset.AssetType,
                        IconKey = asset.IconKey == -9999999 ? null : asset.IconKey,
                        EquipmentVIN = asset.EquipmentVIN == "$#$#$" || String.IsNullOrWhiteSpace(asset.EquipmentVIN) ? null : asset.EquipmentVIN,
                        ModelYear = asset.ModelYear == -9999999 ? null : asset.ModelYear,
                        InsertUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        UpdateUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        StatusInd = 1
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

    public int CreateCustomerUser(string _connectionString, CustomerUserModel customerUser)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into UserCustomer ( fk_UserUID, fk_CustomerUID, StatusInd,LastUserUTC) values ({0}, {1},@StatusInd,@LastUserUTC);"
                      , customerUser.UserUID.ToStringWithoutHyphens().WrapWithUnhex(),
                      customerUser.CustomerUID.ToStringWithoutHyphens().WrapWithUnhex()),
                      new
                      {
                        StatusInd = 1,
                        LastUserUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

    public int CreateCustomerAsset(string _connectionString, CustomerAssetModel customerAsset)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into AssetCustomer ( fk_CustomerUID, fk_AssetUID, LastCustomerUTC) values ({0}, {1},@LastCustomerUTC);"
                      , customerAsset.CustomerUID.ToStringWithoutHyphens().WrapWithUnhex(),
                      customerAsset.AssetUID.ToStringWithoutHyphens().WrapWithUnhex()),
                      new
                      {
                        LastCustomerUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

    public int CreateAssetSummary(string _connectionString, CreateAssetEvent asset)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into AssetSummary ( AssetUID, AssetName, SerialNumber, MakeCode, Model, Family,ModelYear,IconKey,LastAssetSummaryUTC) values ({0}, @AssetName, @SerialNumber, @MakeCode, @Model, @Family,@ModelYear,@IconKey,@LastAssetSummaryUTC);"
                      , asset.AssetUID.ToStringWithoutHyphens().WrapWithUnhex()
                      ),
                      new
                      {
                        AssetName = asset.AssetName == "$#$#$" || String.IsNullOrWhiteSpace(asset.AssetName) ? null : asset.AssetName,
                        SerialNumber = asset.SerialNumber,
                        MakeCode = asset.MakeCode,
                        Model = asset.Model == "$#$#$" || String.IsNullOrWhiteSpace(asset.Model) ? null : asset.Model,
                        Family = asset.AssetType == "$#$#$" || String.IsNullOrWhiteSpace(asset.AssetType) ? null : asset.AssetType,                       
                        ModelYear = asset.ModelYear == -9999999 ? null : asset.ModelYear,
                        IconKey = asset.IconKey == -9999999 ? null : asset.IconKey,
                        LastAssetSummaryUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

    public int CreateSFCustomerUser(string _connectionString, CustomerUserModel customerUser)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into CustomerUser (  fk_CustomerUID, fk_UserUID, LastCustomerUserUTC) values ({0}, {1},@LastCustomerUserUTC);"
                      , customerUser.CustomerUID.ToStringWithoutHyphens().WrapWithUnhex(),
                      customerUser.UserUID.ToStringWithoutHyphens().WrapWithUnhex()),
                      new
                      {
                        LastCustomerUserUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

    public int CreateSFCustomerAsset(string _connectionString, CustomerAssetModel customerAsset)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into CustomerAsset ( fk_AssetUID, fk_CustomerUID,  LastCustomerAssetUTC) values ({0}, {1},@LastCustomerAssetUTC);"
                      , customerAsset.AssetUID.ToStringWithoutHyphens().WrapWithUnhex(),
                      customerAsset.CustomerUID.ToStringWithoutHyphens().WrapWithUnhex()),
                      new
                      {
                        LastCustomerAssetUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

    public int CreateSFAssetStatus(string _connectionString, AssetStatus assetStatus)
    {
      int rowsAffected = 0;


      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();

        try
        {

          rowsAffected = connection.Execute(
                      string.Format("insert into AssetStatus ( fk_AssetUID, Status,  LastStatusUpdateUTC, LastAssetStatusUTC) values ({0}, @Status,@LastStatusUpdateUTC,@LastAssetStatusUTC);"
                      , assetStatus.AssetUID.ToStringWithoutHyphens().WrapWithUnhex()
                      ),
                      new
                      {
                        Status=assetStatus.Status,
                        LastStatusUpdateUTC = assetStatus.LastStatusUpdateUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                        LastAssetStatusUTC = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                      });

          connection.Close();

        }
        catch (MySqlException ex)
        {
          if (!ex.Message.Contains("Duplicate"))
            throw;
        }
        finally
        {
          connection.Close();
        }
      }
      return rowsAffected;
    }

  }
}
