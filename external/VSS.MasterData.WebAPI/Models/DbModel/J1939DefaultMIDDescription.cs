using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.DbModel
{
    public partial class J1939DefaultMIDDescription
    {
        #region Primitive Properties

        public virtual long ID
        {
            get;
            set;
        }

        public virtual bool? ArbitraryAddressCapable
        {
            get;
            set;
        }

        public virtual byte? IndustryGroup
        {
            get;
            set;
        }

        public virtual byte? VehicleSystemInstance
        {
            get;
            set;
        }

        public virtual byte? VehicleSystem
        {
            get;
            set;
        }

        public virtual byte? J1939Function
        {
            get;
            set;
        }

        public virtual byte? FunctionInstance
        {
            get;
            set;
        }

        public virtual byte? ECUInstance
        {
            get;
            set;
        }

        public virtual int? ManufacturerCode
        {
            get;
            set;
        }

        public virtual int? IdentityNumber
        {
            get;
            set;
        }

        public virtual int fk_LanguageID
        {
            get;
            set;
        }

        public virtual string Name
        {
            get;
            set;
        }

        #endregion

    }
}
