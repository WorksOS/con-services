using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.Hosted.VLCommon
{
    public class AssetModelInformation
    {
        public long SalesModelId { get; set; }
        public string SalesModelCode { get; set; }
        public string SalesModelDescription { get; set; }
        public long ProductFamilyId { get; set; }
        public string ProductFamilyName { get; set; }
        public string ProductFamilyDescription { get; set; }
        public int IconId { get; set; }
    }

    public class OemAssetInformationStrategy
    {
        private void Init()
        {
            //
            // ADD NEW STRATEGIES HERE
            //
            _strategies.Add("CAT", new CatAssetInformationStrategy(_ctx));           // Cat
            _strategies.Add("F80", new CatAssetInformationStrategy(_ctx));           // F.G. Wilson (same as Cat)
            _strategies.Add("O80", new CatAssetInformationStrategy(_ctx));           // Olympian (same as Cat)
            _strategies.Add("VER", new VermeerAssetInformationStrategy(_ctx));       // Vermeer
            _strategies.Add("THC", new THCAssetInformationStrategy(_ctx));          // Tata Hitachi
            _strategies.Add("LBY", new LeeBoyAssetInformationStrategy(_ctx));       // LeeBoy
            _strategies.Add("CIH", new CaseAssetInformationStrategy(_ctx));       // CASE    
            _strategies.Add("NH", new NHAssetInformationStrategy(_ctx));       // NEW HOLLAND    
            _strategies.Add("DSN", new DSNAssetInformationStrategy(_ctx));       // DOOSAN
            _strategies.Add("LTCEL", new LTCELAssetInformationStrategy(_ctx));   //L&T
            _strategies.Add("LGI", new LiuGongAssetInformationStrategy(_ctx));   //LIUGONG
            _strategies.Add("KDC", new LTCMAssetInformationStrategy(_ctx));   //LTCM-KOMATSU
            _strategies.Add("ALCV", new ALCVAssetInformationStrategy(_ctx));  //ALCV
        }

        private readonly INH_OP _ctx;
        private readonly IDictionary<string, IAssetInformationStrategy> _strategies = new Dictionary<string, IAssetInformationStrategy>();

        public OemAssetInformationStrategy(INH_OP ctx)
        {
            if (ctx == null)
                throw new ArgumentNullException("ctx");

            _ctx = ctx;

            Init();
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            return GetStrategy(makeCode).GetAssetModelInformation(makeCode, serialNumber);
        }

        public void UpdateAsset(Asset asset)
        {
            if (asset == null)
                throw new ArgumentNullException("asset");

            GetStrategy(asset.fk_MakeCode).UpdateAsset(asset);
        }

        public IAssetInformationStrategy GetStrategy(string makeCode)
        {
            IAssetInformationStrategy strategy;
            if (_strategies.TryGetValue(makeCode, out strategy))
            {
                // Apply strategy
                return strategy;
            }

            return new NullAssetInformationStrategy();
        }
    }

    public interface IAssetInformationStrategy
    {
        /// <summary>
        /// Seting UseStoreInformation to true in an IAssetInformationStrategy implementation 
        /// causes EquipmentAPI.UpdateModelAndProductFamily to use received BSS model and 
        /// product family values instead of current database values
        /// </summary>
        bool UseStoreInformation { get; set; }

        void UpdateAsset(Asset asset);
        AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber);
    }

    public class NullAssetInformationStrategy : IAssetInformationStrategy
    {
        public void UpdateAsset(Asset asset)
        {
            // No Implementation
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            return new AssetModelInformation();
        }

        public bool UseStoreInformation { get; set; }
    }

    public class CatAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public CatAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);

            //UpdateAssetWithAssetOptions(asset);

            UpdateModelVariant(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || serialNumber.Length < 4)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            if (serialNumber.Length == 9 && serialNumber.StartsWith("0"))
                serialNumber = serialNumber.Substring(1);

            string serialNumberPrefix = serialNumber.Substring(0, 3);

            long serialNumberPostfix;
            if (!long.TryParse(serialNumber.Substring(3), out serialNumberPostfix))
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.SerialNumberPrefix == serialNumberPrefix
                             && sm.StartRange.HasValue
                             && serialNumberPostfix >= sm.StartRange.Value
                             && sm.EndRange.HasValue
                             && serialNumberPostfix <= sm.EndRange.Value
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.Default
                             }).SingleOrDefault();

            if (null == modelInfo)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            UseStoreInformation = false;
            return modelInfo;
        }

        private const string WtsProductFamilyName = "WHEEL TRACTOR SCRAPERS";
        private void UpdateModelVariant(Asset asset)
        {
            int modelVariant = 0;
            if (!string.IsNullOrEmpty(asset.Model) && asset.Model.Length > 2 && !string.IsNullOrEmpty(asset.ProductFamilyName))
            {
                if (asset.Model.Substring(2, 1) == "7" && asset.ProductFamilyName.Equals(WtsProductFamilyName, StringComparison.InvariantCultureIgnoreCase))
                    modelVariant = (int)ModelVariantEnum.DualEngine;
            }
            asset.fk_ModelVariant = modelVariant;
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

            if (assetModelInformation.SalesModelDescription != null)
            {
                asset.Model = assetModelInformation.SalesModelCode;
            }

            if (assetModelInformation.ProductFamilyDescription != null)
            {
                asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
            }

            asset.IconID = assetModelInformation.IconId;
        }

        public bool UseStoreInformation { get; set; }
    }

    public class VermeerAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public VermeerAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
            UseStoreInformation = true;
        }

        public void UpdateAsset(Asset asset)
        {
            // Since UseStoreInformation is true, there is no need to read product family/model information from the database.
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            // There is no Vermeer product family or model information in the database.
            return new AssetModelInformation();
        }

        public bool UseStoreInformation { get; set; }
    }

    #region Tata Hitachi
    public class THCAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public THCAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {

            UpdateAssetWithModelAndProductFamily(asset);
        }
        
        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || serialNumber.Length < 5)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            if (serialNumber.Length == 11 && serialNumber.StartsWith("0"))
                serialNumber = serialNumber.Substring(1);

            var prefixLength = GetSerialNumberPrefixLength(serialNumber);
            string serialNumberPrefix = serialNumber.Substring(0, prefixLength);
            string alternateSerialNumberPrefix = serialNumber.Substring(0, 7);

            long serialNumberPostfix, alternateSerialNumberPostfix;
            if (!(long.TryParse(serialNumber.Substring(prefixLength), out serialNumberPostfix)
                | long.TryParse(serialNumber.Substring(7), out alternateSerialNumberPostfix)))
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.StartRange.HasValue
                             && sm.EndRange.HasValue
                             && (sm.SerialNumberPrefix == serialNumberPrefix
                             && serialNumberPostfix >= sm.StartRange.Value
                             && serialNumberPostfix <= sm.EndRange.Value)
                             || (serialNumber.Length == 11
                             && sm.SerialNumberPrefix == alternateSerialNumberPrefix
                             && alternateSerialNumberPostfix >= sm.StartRange.Value
                             && alternateSerialNumberPostfix <= sm.EndRange.Value)
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             })?.SingleOrDefault();

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }
        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

            if (assetModelInformation.SalesModelCode != null)
            {
                asset.Model = assetModelInformation.SalesModelCode;
            }

            if (assetModelInformation.ProductFamilyDescription != null)
            {
                asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
            }

            asset.IconID = assetModelInformation.IconId;
        }

        public bool UseStoreInformation { get; set; }

        private int GetSerialNumberPrefixLength(string serialNumber)
        {
            var snlength = serialNumber.Length;
            return snlength == 17 ? 10 : (snlength == 13 ? 6 : (snlength == 11 ? 8 : 7));
        }
    }
    #endregion //Tata Hitachi

    #region LeeBoy
    public class LeeBoyAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public LeeBoyAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {

            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || serialNumber.Length != 12)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            string serialNumberPrefix = serialNumber.Substring(0, 2);

            long serialNumberPostfix;
            if (!long.TryParse(serialNumber.Substring(2), out serialNumberPostfix))
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.SerialNumberPrefix == serialNumberPrefix
                             && sm.StartRange.HasValue
                             && serialNumberPostfix >= sm.StartRange.Value
                             && sm.EndRange.HasValue
                             && serialNumberPostfix <= sm.EndRange.Value
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).SingleOrDefault();

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }
        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

            if (assetModelInformation.SalesModelCode != null)
            {
                asset.Model = assetModelInformation.SalesModelCode;
            }

            if (assetModelInformation.ProductFamilyDescription != null)
            {
                asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
            }

            asset.IconID = assetModelInformation.IconId;
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion //LeeBoy

    #region CASE 

    public class CaseAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public CaseAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            UseStoreInformation = true;
            return new AssetModelInformation();
        }

        private AssetModelInformation GetAssetProductFamilyFromModel(string model)
        {
            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.ModelCode == model
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).SingleOrDefault();

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            var assetProductFamilyInformation = GetAssetProductFamilyFromModel(asset.Model);

            if (assetProductFamilyInformation.ProductFamilyDescription != null)
            {
                asset.ProductFamilyName = assetProductFamilyInformation.ProductFamilyDescription;
            }

            asset.IconID = assetProductFamilyInformation.IconId;
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion CASE //CASE

    #region NEW HOLLAND

    public class NHAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public NHAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            UseStoreInformation = true;
            return new AssetModelInformation();
        }

        private AssetModelInformation GetAssetProductFamilyFromModel(string model)
        {
            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.ModelCode == model
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).SingleOrDefault();


            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            var assetProductFamilyInformation = GetAssetProductFamilyFromModel(asset.Model);

            if (assetProductFamilyInformation.ProductFamilyDescription != null)
            {
                asset.ProductFamilyName = assetProductFamilyInformation.ProductFamilyDescription;
            }

            asset.IconID = assetProductFamilyInformation.IconId;
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion NEW HOLLAND //NEW HOLLAND

    #region DOOSAN

    public class DSNAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public DSNAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || serialNumber.Length != 11)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            string serialNumberPrefix = serialNumber.Substring(0, 5);

            long serialNumberPostfix;
            if (!long.TryParse(serialNumber.Substring(5), out serialNumberPostfix))
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.SerialNumberPrefix == serialNumberPrefix
                             && sm.StartRange.HasValue
                             && serialNumberPostfix >= sm.StartRange.Value
                             && sm.EndRange.HasValue
                             && serialNumberPostfix <= sm.EndRange.Value
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).FirstOrDefault();//Will be replaced with singleordefault when it is decided how to handle two models with same prefix 

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return modelInfo ?? new AssetModelInformation();
            }
        }

        private AssetModelInformation GetAssetProductFamilyFromModel(string model)
        {
            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.ModelCode == model
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).SingleOrDefault();

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            if (string.IsNullOrEmpty(asset.Model))//if model is null then process serialnumber for model info
            {
                var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

                if (assetModelInformation.SalesModelCode != null)
                {
                    asset.Model = assetModelInformation.SalesModelCode;
                }

                if (assetModelInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
                }

                asset.IconID = assetModelInformation.IconId;

            }
            else
            {
                var assetProductFamilyInformation = GetAssetProductFamilyFromModel(asset.Model);

                if (assetProductFamilyInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetProductFamilyInformation.ProductFamilyDescription;
                }

                asset.IconID = assetProductFamilyInformation.IconId;
            }
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion DOOSAN //DOOSAN

    #region L&T

    public class LTCELAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public LTCELAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || (serialNumber.Length != 5
                && serialNumber.Length != 6 && serialNumber.Length != 17))
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            string serialNumberPrefix = null;
            long serialNumberPostfix = 0;

            if (serialNumber.Length == 5)
            {
                serialNumberPrefix = serialNumber.Substring(0, 1);
                long.TryParse(serialNumber.Substring(1), out serialNumberPostfix);
            }

            if (serialNumber.Length == 6)
            {
                serialNumberPrefix = serialNumber.Substring(0, 2);
                long.TryParse(serialNumber.Substring(2), out serialNumberPostfix);
            }

            if (serialNumber.Length == 17)
            {
                serialNumberPrefix = serialNumber.Substring(0, 9);
                long.TryParse(serialNumber.Substring(13), out serialNumberPostfix);
            }

            if (String.IsNullOrEmpty(serialNumberPrefix) || serialNumberPostfix < 1)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }


            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.SerialNumberPrefix == serialNumberPrefix
                             && sm.StartRange.HasValue
                             && serialNumberPostfix >= sm.StartRange.Value
                             && sm.EndRange.HasValue
                             && serialNumberPostfix <= sm.EndRange.Value
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).FirstOrDefault();//Will be replaced with singleordefault when it is decided how to handle two models with same prefix 

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private AssetModelInformation GetAssetProductFamilyFromModel(string model)
        {
            AssetModelInformation modelInfo = null;
            var modelInfoArray = from sm in _ctx.SalesModelReadOnly
                                 join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                                 where sm.ModelCode == model
                                 && sm.fk_ProductFamilyID != 0
                                 select new AssetModelInformation
                                 {
                                     SalesModelDescription = sm.Description,
                                     ProductFamilyId = pf.ID,
                                     ProductFamilyName = pf.Name,
                                     ProductFamilyDescription = pf.Description,
                                     IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                                 };
            //Fix for multiple model with same name-- 1190 model bug#34528 
            if (modelInfoArray.Count() == 1)
            {
                modelInfo = modelInfoArray.SingleOrDefault();
            }
            else if (modelInfoArray.Count() > 1)
            {
                modelInfo = (from i in _ctx.IconReadOnly
                             join mi in modelInfoArray on i.ID equals mi.IconId
                             where i.fk_MakeCode == MakeEnum.LTCEL.ToString()
                             select mi).SingleOrDefault();
            }

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            if (string.IsNullOrEmpty(asset.Model))//if model is null then process serialnumber for model info
            {
                var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

                if (assetModelInformation.SalesModelCode != null)
                {
                    asset.Model = assetModelInformation.SalesModelCode;
                }

                if (assetModelInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
                }

                asset.IconID = assetModelInformation.IconId;

            }
            else
            {
                var assetProductFamilyInformation = GetAssetProductFamilyFromModel(asset.Model);

                if (assetProductFamilyInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetProductFamilyInformation.ProductFamilyDescription;
                }

                asset.IconID = assetProductFamilyInformation.IconId;
            }
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion L&T

    #region LTCM

    public class LTCMAssetInformationStrategy : IAssetInformationStrategy
    {
        private const string KOMATSUMAKECODE = "KDC";
        private readonly INH_OP _ctx;

        public LTCMAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {


            if (string.IsNullOrWhiteSpace(serialNumber) || serialNumber.Length < 7 || serialNumber.Length > 8)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            string serialNumberPrefix = string.Empty;
            long serialNumberPostfix = 0;

            if (serialNumber.Length == 7)
            {
                serialNumberPrefix = serialNumber.Substring(0, 2);
                long.TryParse(serialNumber.Substring(2), out serialNumberPostfix);
            }
            if (serialNumber.Length == 8)
            {
                serialNumberPrefix = KOMATSUMAKECODE + serialNumber.Substring(0, 3);
                long.TryParse(serialNumber.Substring(3), out serialNumberPostfix);
            }

            if (String.IsNullOrEmpty(serialNumberPrefix) || serialNumberPostfix < 1)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.SerialNumberPrefix == serialNumberPrefix
                             && sm.StartRange.HasValue
                             && serialNumberPostfix >= sm.StartRange.Value
                             && sm.EndRange.HasValue
                             && serialNumberPostfix <= sm.EndRange.Value
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).SingleOrDefault();

            if (modelInfo != null)
                return modelInfo;
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private AssetModelInformation GetAssetProductFamilyFromModel(string model)
        {
            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.ModelCode == model
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).FirstOrDefault();

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            if (string.IsNullOrEmpty(asset.Model))//if model is null then process serialnumber for model info
            {
                var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

                if (assetModelInformation.SalesModelCode != null)
                {
                    asset.Model = assetModelInformation.SalesModelCode;
                }

                if (assetModelInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
                }

                asset.IconID = assetModelInformation.IconId;

            }
            else
            {
                var assetProductFamilyInformation = GetAssetProductFamilyFromModel(asset.Model);

                if (assetProductFamilyInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetProductFamilyInformation.ProductFamilyDescription;
                }

                asset.IconID = assetProductFamilyInformation.IconId;
            }
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion LTCM

    #region LiuGong
    public class LiuGongAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public LiuGongAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {
            UpdateAssetWithModelAndProductFamily(asset);
        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || serialNumber.Length != 17)
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            string serialNumberPrefix = serialNumber.Substring(0, 11);

            long serialNumberPostfix;
            if (!long.TryParse(serialNumber.Substring(11), out serialNumberPostfix))
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }

            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.SerialNumberPrefix == serialNumberPrefix
                             && sm.StartRange.HasValue
                             && serialNumberPostfix >= sm.StartRange.Value
                             && sm.EndRange.HasValue
                             && serialNumberPostfix <= sm.EndRange.Value
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelId = sm.ID,
                                 SalesModelCode = sm.ModelCode,
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).FirstOrDefault();//Will be replaced with singleordefault when it is decided how to handle two models with same prefix 

            if (modelInfo != null)
                return modelInfo;
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private AssetModelInformation GetAssetProductFamilyFromModel(string model)
        {
            var modelInfo = (from sm in _ctx.SalesModelReadOnly
                             join pf in _ctx.ProductFamilyReadOnly on sm.fk_ProductFamilyID equals pf.ID
                             where sm.ModelCode == model
                             && sm.fk_ProductFamilyID != 0
                             select new AssetModelInformation
                             {
                                 SalesModelDescription = sm.Description,
                                 ProductFamilyId = pf.ID,
                                 ProductFamilyName = pf.Name,
                                 ProductFamilyDescription = pf.Description,
                                 IconId = sm.fk_IconID ?? (int)IconEnum.GenericNonCAT
                             }).SingleOrDefault();

            if (modelInfo != null)
            {
                return modelInfo;
            }
            else
            {
                UseStoreInformation = true;
                return new AssetModelInformation();
            }
        }

        private void UpdateAssetWithModelAndProductFamily(Asset asset)
        {
            if (string.IsNullOrEmpty(asset.Model))//if model is null then process serialnumber for model info
            {
                var assetModelInformation = GetAssetModelInformation(asset.fk_MakeCode, asset.SerialNumberVIN);

                if (assetModelInformation.SalesModelDescription != null)
                {
                    asset.Model = assetModelInformation.SalesModelCode;
                }

                if (assetModelInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetModelInformation.ProductFamilyDescription;
                }

                asset.IconID = assetModelInformation.IconId;

            }
            else
            {
                var assetProductFamilyInformation = GetAssetProductFamilyFromModel(asset.Model);

                if (assetProductFamilyInformation.ProductFamilyDescription != null)
                {
                    asset.ProductFamilyName = assetProductFamilyInformation.ProductFamilyDescription;
                }

                asset.IconID = assetProductFamilyInformation.IconId;
            }
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion LiuGong

    #region Ashok Leyland
    public class ALCVAssetInformationStrategy : IAssetInformationStrategy
    {
        private readonly INH_OP _ctx;

        public ALCVAssetInformationStrategy(INH_OP ctx)
        {
            _ctx = ctx;
        }

        public void UpdateAsset(Asset asset)
        {

        }

        public AssetModelInformation GetAssetModelInformation(string makeCode, string serialNumber)
        {
            UseStoreInformation = true;
            return new AssetModelInformation();
        }

        public bool UseStoreInformation { get; set; }
    }
    #endregion Ashok Leyland
}

