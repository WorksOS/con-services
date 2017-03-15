using System;

namespace Repositories.DBModels
{
  public class CustomerUser
  {
    public string UserUID { get; set; }
    public string CustomerUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}
