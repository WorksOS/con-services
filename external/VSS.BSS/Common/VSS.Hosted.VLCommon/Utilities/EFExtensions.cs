using System;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
	public static class EFExtensions
	{
		public static int? GetColumnMaxLength(this ObjectContext context, Type entityType, string columnName)
		{

			var q = from meta in context.MetadataWorkspace.GetItems(DataSpace.CSpace)
												.Where(m => m.BuiltInTypeKind == BuiltInTypeKind.EntityType)
							from p in (meta as EntityType).Properties
							.Where(p => p.Name == columnName
													&& p.TypeUsage.EdmType.Name == "String")
							select p;

			var queryResult = q.Where(p =>
			{
				bool match = p.DeclaringType.Name == entityType.FullName;
				if (!match && entityType != null)
				{
					//Is a fully qualified name....
					match = entityType.Name == p.DeclaringType.Name;
				}

				return match;

			}).Select(sel => sel.TypeUsage.Facets["MaxLength"].Value);
			if (queryResult.Any())
			{
				string val = queryResult.First().ToString();
				int ret;
				bool isInt = Int32.TryParse(val, out ret);
				if (isInt)
					return ret;
				if (val.Equals("Max"))
					return int.MaxValue;
				return null;
			}

			return null;
		}
	}
}
