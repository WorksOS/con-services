using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerWebApi.Models
{
  public class AccountsResult
  {
    public string UserUID;
    public List<Customers> Customers = new List<Customers>();
  }

  public class Customers
  {
    public string CustomerUID;
    public string Name; // ": "CATERPILLAR DEMO DEALER TD00",
    public string CustomerType; //": "Dealer",
    public string CustomerCode; //": "TD00",
    public string DisplayName; // (TD00) CATERPILLAR DEMO DEALER TD00",
    public List<Customers> Children = new List<Customers>(); 
  }
}

//{
//  "UserUID": "e55e503c-b7f2-44ec-9c23-b7b0f930b1ee",
//  "Customers": [
//    {
//      "CustomerUID": "8abcf851-44c5-e311-aa77-00505688274d",
//      "Name": "CATERPILLAR DEMO DEALER TD00",
//      "CustomerType": "Dealer",
//      "CustomerCode": "TD00",
//      "DisplayName": "(TD00) CATERPILLAR DEMO DEALER TD00",
//      "Children": [


//      ]
//    },
//    {
//      "CustomerUID": "05c7d037-a23d-11e7-8110-02baf17be4cb",
//      "Name": "3D Alpha Testing",
//      "CustomerType": "Customer",
//      "DisplayName": "3D Alpha Testing",
//      "Children": [


//      ]
//    }
//  ]
//}
