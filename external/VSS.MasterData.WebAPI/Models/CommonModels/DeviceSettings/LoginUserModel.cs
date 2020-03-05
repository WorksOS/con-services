using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class LoginUserModel
    {
        public Guid? UserUID { get; set; }
        public string EMaildID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int StatusInd { get; set; }
        public DateTime LastUserUTC { get; set; }
    }
}
