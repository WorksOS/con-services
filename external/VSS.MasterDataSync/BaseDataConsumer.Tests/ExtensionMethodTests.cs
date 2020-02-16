using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using VSS.Messaging.BaseDataConsumer;
using VSS.Messaging.BaseDataConsumer.Destination.Database;
using VSS.Messaging.BaseDataConsumer.Destination.Objects;
using Xunit;

namespace BaseDataConsumer.Tests
{
	public class ExtensionMethodTests
	{
		[Fact]
		public void ProductFamily_ValidatingRowsInResultDataTable()
		{
			List<object> list = new List<object>();
			list.Add((object)new ProductFamilyDto
			{
				Description = "FOREST PRODUCT SWING MACHINE",
				Name = "FPSM",
				ProductFamilyUID = new Guid("30323130-3038-6366-2d63-3130342d3131")
			});

			DataTable expectedDataTable = new DataTable();
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "Description", DataType = typeof(string) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "Name", DataType = typeof(string) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "ProductFamilyUID", DataType = typeof(Guid) });

			DataRow expectedDataRow = expectedDataTable.NewRow();
			expectedDataRow["Description"] = "FOREST PRODUCT SWING MACHINE";
			expectedDataRow["Name"] = "FPSM";
			expectedDataRow["ProductFamilyUID"] = "30323130-3038-6366-2d63-3130342d3131";
			expectedDataTable.Rows.Add(expectedDataRow);

			DataTable result = new DataTable();
			list.CopyToDataTable(result, LoadOption.PreserveChanges);

			int noOfRowsMatched = 0;
			var array1 = result.Rows[0].ItemArray;
			var array2 = result.Rows[0].ItemArray;
			if (array1.SequenceEqual(array2))
			{
				noOfRowsMatched = 1;
			}

			Assert.Equal(1, noOfRowsMatched);
		}

		[Fact]
		public void SalesModel_ValidatingRowsInResultDataTable()
		{
			List<object> list = new List<object>();
			list.Add((object)new SalesModelDto
			{
				ModelCode = "A09",
				SerialNumberPrefix = "A09",
				StartRange = 1,
				EndRange = 999,
				Description = "A09",
				IconUID = new Guid("34373838-6264-3264-2d63-3130322d3131"),
				ProductFamilyUID = new Guid("30323134-6233-3435-2d63-3130342d3131"),
				SalesModelUID = new Guid("0790880c-78c2-49b6-bef7-61fd99695d5d")
			});

			DataTable expectedDataTable = new DataTable();
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "ModelCode", DataType = typeof(string) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "SerialNumberPrefix", DataType = typeof(string) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "StartRange", DataType = typeof(long) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "EndRange", DataType = typeof(long) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "Description", DataType = typeof(string) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "IconUID", DataType = typeof(Guid) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "ProductFamilyUID", DataType = typeof(Guid) });
			expectedDataTable.Columns.Add(new DataColumn() { ColumnName = "SalesModelUID", DataType = typeof(Guid) });

			DataRow expectedDataRow = expectedDataTable.NewRow();
			expectedDataRow["ModelCode"] = "A09";
			expectedDataRow["SerialNumberPrefix"] = "A09";
			expectedDataRow["StartRange"] = "1";
			expectedDataRow["EndRange"] = "999";
			expectedDataRow["Description"] = "A09";
			expectedDataRow["IconUID"] = "34373838-6264-3264-2d63-3130322d3131";
			expectedDataRow["ProductFamilyUID"] = "30323134-6233-3435-2d63-3130342d3131";
			expectedDataRow["SalesModelUID"] = "0790880c-78c2-49b6-bef7-61fd99695d5d";
			expectedDataTable.Rows.Add(expectedDataRow);

			DataTable result = list.CopyToDataTable();

			int noOfRowsMatched = 0;
			var array1 = result.Rows[0].ItemArray;
			var array2 = result.Rows[0].ItemArray;
			if (array1.SequenceEqual(array2))
			{
				noOfRowsMatched = 1;
			}

			Assert.Equal(1, noOfRowsMatched);
		}
	}
}