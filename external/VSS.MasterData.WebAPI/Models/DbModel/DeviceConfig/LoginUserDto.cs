using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
    public class LoginUserDto
    {
        private string _UserUIDString;
        public string UserUIDString
        {
            get { return _UserUIDString; }
            set { _UserUIDString = value; }
        }
        public Guid UserUID
        {
            get { return Guid.Parse(_UserUIDString); }
            set { _UserUIDString = value.ToString("N"); }
        }
        public string EmailID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int StatusInd { get; set; }
        public DateTime LastUserUTC { get; set; }
    }
}
