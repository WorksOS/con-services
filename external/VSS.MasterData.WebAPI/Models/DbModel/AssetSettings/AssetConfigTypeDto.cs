using CommonModel.Enum;
using System;
using System.Collections.Generic;

namespace DbModel.AssetSettings
{
	public class AssetConfigTypeDto
    {
        public AssetTargetType AssetTargetType
        {
            get
            {
                return (AssetTargetType)Enum.Parse(typeof(AssetTargetType), this.ConfigTypeName);
            }
        }
        public int AssetConfigTypeID { get; set; }
        public IEnumerable<string> ConfigTypeNames { get; set; }
        public string ConfigTypeName { get; set; }
        public string ConfigTypeDescr { get; set; }
        public DateTime InsertUTC { get; set; }
        public DateTime UpdateUTC { get; set; }
    }
}
