using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

namespace VSS.Nighthawk.NHBssSvc
{
  public class CustomHttpMessageInspector : IClientMessageInspector
  {

    #region Properties

    private const string USER_AGENT_HTTP_HEADER = "User-Agent";
    private const string CONTENT_TYPE_HTTP_HEADER = "Content-Type";
    private const string AUTHORIZATION_HTTP_HEADER = "Authorization";
    private const string SOAP_ACTION_HTTP_HEADER = "SOAPAction";

    private const string USER_AGENT_HTTP_VALUE = "VSS";
    private const string CONTENT_TYPE_HTTP_VALUE = "text/xml";
    private const string SOAP_ACTION_HTTP_VALUE = "\"process\"";

    private string userName;
    private string passWord;
    private string soapAction;

    /// <summary>
    /// Username to be used in the communication
    /// </summary>
    public string UserName
    {
      set
      {
        if (!string.IsNullOrWhiteSpace(value) && string.Compare(value, userName, true) != 0)
          userName = value;
      }
    }

    /// <summary>
    /// Password to be used in the communication
    /// </summary>
    public string Password
    {
      set
      {
        if (!string.IsNullOrWhiteSpace(value) && string.Compare(value, passWord, true) != 0)
          passWord = value;
      }
    }

    /// <summary>
    /// Set the SOAP action if it is other than "process"
    /// "process" is the default SOAP action returned by this property
    /// </summary>
    public string SOAPAction
    {
      protected get
      {
        if (!string.IsNullOrWhiteSpace(soapAction))
          return soapAction;
        return SOAP_ACTION_HTTP_VALUE;
      }
      set
      {
        if (!string.IsNullOrWhiteSpace(value) && string.Compare(value, soapAction, true) != 0)
          soapAction = value;
      }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Set the header properties of the message
    /// </summary>
    /// <param name="request"></param>
    /// <param name="attribute"></param>
    /// <param name="value"></param>
    private void SetHeaderProperty(ref System.ServiceModel.Channels.Message request, string attribute, string value)
    {
      HttpRequestMessageProperty httpRequestMessage;
      object httpRequestMessageObject;

      if (request.Properties.TryGetValue(HttpRequestMessageProperty.Name, out httpRequestMessageObject))
      {
        httpRequestMessage = httpRequestMessageObject as HttpRequestMessageProperty;
        if (string.IsNullOrWhiteSpace(httpRequestMessage.Headers[attribute]))
          httpRequestMessage.Headers[attribute] = value;
      }
      else
      {
        httpRequestMessage = new HttpRequestMessageProperty();
        httpRequestMessage.Headers.Add(attribute, value);
        request.Properties.Add(HttpRequestMessageProperty.Name, httpRequestMessage);
      }
    }

    /// <summary>
    /// takes username and password and constructs a base64 encoded string
    /// which is passed as part of the soap header of the message
    /// </summary>
    /// <param name="userName">username to be encoded</param>
    /// <param name="passWord">password to be encoded</param>
    /// <returns>base64 encoded string</returns>
    private static string GetAuthorizationToken(string userName, string passWord)
    {
      byte[] authbytes = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", userName, passWord));
      string base64 = Convert.ToBase64String(authbytes);
      return string.Format("Basic {0}", base64);
    }

    #endregion

    #region IClientMessageInspector Implementation Methods

    /// <summary>
    /// override this method to add any custom headers to the soap message
    /// or to override the default values of the soap headers
    /// </summary>
    /// <param name="request"></param>
    /// <param name="channel"></param>
    /// <returns></returns>
    public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel)
    {
      SetHeaderProperty(ref request, USER_AGENT_HTTP_HEADER, USER_AGENT_HTTP_VALUE);
      SetHeaderProperty(ref request, CONTENT_TYPE_HTTP_HEADER, CONTENT_TYPE_HTTP_VALUE);
      SetHeaderProperty(ref request, SOAP_ACTION_HTTP_HEADER, SOAP_ACTION_HTTP_VALUE);
      SetHeaderProperty(ref request, AUTHORIZATION_HTTP_HEADER, GetAuthorizationToken(userName, passWord));
      return null;
    }

    //wantedly removed "throw new NotImplementedException()" as we don't have any custom 
    //implementation for the below methods and don't want to fail custom behaviour for that reason.
    public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState) { }

    #endregion
  }
}
