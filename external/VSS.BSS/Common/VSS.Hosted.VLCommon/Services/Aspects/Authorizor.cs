using System;
using System.Collections.Generic;
using System.Linq;
using AopAlliance.Intercept;
using System.Reflection;
using System.Runtime.Caching;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public class Authorizor : IMethodInterceptor
  {
    private static MemoryCache featureCache = null;
    #region IMethodInterceptor Members

    public object Invoke(IMethodInvocation invocation)
    {
      MethodInfo mi = invocation.Method;
      object[] attributes = mi.GetCustomAttributes(typeof(AuthorizorAttribute), false);
      if (attributes != null)
      {
        // Special case
        if (mi.Name == "Login")
        {
          string userName = RetrieveUserName(mi.GetParameters(), invocation);
          if (!string.IsNullOrEmpty(userName))
          {
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
              // don't need to check Active, etc, and correct pwd - just a quick lookup for the userID is all we need for this step.
              var userID = (from u in ctx.UserReadOnly where u.Name == userName select u.ID).SingleOrDefault();
              if (userID > 0)
              {
                CheckAuthorization(ctx, userID, attributes);
              }
            }
          }
        }
        else
        {
          string sessionID = RetrieveSessionID(mi.GetParameters(), invocation);
          if (!string.IsNullOrEmpty(sessionID))
          {
            SessionContext sesh = API.Session.Validate(sessionID);

            if (null != sesh && sesh.UserID.HasValue)
            {
              CheckAuthorization(sesh.NHOpContext, sesh.UserID.Value, attributes);
            }
          }
        }
      }

      return invocation.Proceed(); 
    }

    private string RetrieveSessionID(ParameterInfo[] parameterInfo, IMethodInvocation invocation)
    {
      foreach (ParameterInfo param in parameterInfo)
      {
        if (param.Name.ToLower().Contains("sessionid"))  // could be "dealerSessionID", etc
        {
          return invocation.Arguments[param.Position].ToString();
        }
      }
      return string.Empty;
    }

    private string RetrieveUserName(ParameterInfo[] parameterInfo, IMethodInvocation invocation)
    {
      foreach (ParameterInfo param in parameterInfo)
      {
        if (param.Name.ToLower() == "username")
        {
          return invocation.Arguments[param.Position].ToString();
        }
      }
      return string.Empty;
    }

    private void CheckAuthorization(INH_OP ctx, long userID, object[] attributes)
    {
      string feature = null;
      bool success = HasAuthorization(ctx, userID, attributes, out feature);
 
      if (!success)
      {
        throw new UnauthorizedAccessException(string.Format("Access Denied\nUser does not have the following permission(s):\n{0}", feature));
      }
    }

    public bool HasAuthorization(INH_OP ctx, long userID, object[] attributes, out string feature)
    {
      bool success = true;
      feature = string.Empty;

      if (attributes != null)
      {
        List<UserFeature> userFeatures = GetUserFeatures(ctx, userID);

        foreach (AuthorizorAttribute a in attributes)
        {
          success = CheckPermission(userFeatures, a);

          if (success)
          {
            break;
          }
          else
          {
            if (!string.IsNullOrEmpty(feature))
              feature += ", ";
            if ((int)a.FeatureChild != 0)
            {
              feature += a.FeatureChild.ToString();
            }
            else if (a.FeatureChildren != null)
            {
              for (int i = 0; i < a.FeatureChildren.Length; i++)
              {
                if (i > 0)
                   feature += ", ";
                feature += a.FeatureChildren[i].ToString();
               }
            }
            else if ((int)a.Feature != 0)
            {
              feature += a.Feature.ToString();
            }
            else
            {
              feature += a.FeatureApp.ToString();
            }
            feature += " " + a.FeatureAccess.ToString();
          }
        }

      }
      return success;
    }

    private static List<UserFeature> GetUserFeatures(INH_OP ctx, long userID)
    {
      if (null == featureCache)
        featureCache = new MemoryCache("UserFeatureByUserID");

      List<UserFeature> userFeatures = (List<UserFeature>)featureCache.Get(userID.ToString());

      if (null == userFeatures)
      {
        userFeatures = (from uf in ctx.UserFeatureReadOnly
                        where uf.fk_User == userID
                        orderby uf.fk_Feature descending
                        select uf).ToList();

        featureCache.Add(new CacheItem(userID.ToString(), userFeatures), new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(2.0) });
      }

      return userFeatures;
    }

    private bool CheckPermission(List<UserFeature> userFeatures, AuthorizorAttribute authorizationAtt)
    {

      //permissions cascade up unless it is overridden they do not cascade down
      foreach (UserFeature uf in userFeatures)
      {
        //If multiple children specified then user can have any one of them to be authorized
        if (authorizationAtt.FeatureChildren != null)
        {
          foreach (FeatureChildEnum fc in authorizationAtt.FeatureChildren)
          {
            if ((int)fc == uf.fk_Feature)
            {
              if ((int)authorizationAtt.FeatureAccess <= uf.fk_FeatureAccess)
                return true;
            }
          }
        }
        //Note not an "else if" to allow for cascading permissions
        if ((int)authorizationAtt.FeatureChild == uf.fk_Feature)
        {
          return (int)authorizationAtt.FeatureAccess <= uf.fk_FeatureAccess;
        }
        else if ((int)authorizationAtt.Feature == uf.fk_Feature)
        {
          return (int)authorizationAtt.FeatureAccess <= uf.fk_FeatureAccess;
        }
        else if ((int)authorizationAtt.FeatureApp == uf.fk_Feature)
        {
          return (int)authorizationAtt.FeatureAccess <= uf.fk_FeatureAccess;
        }
      }
      return false;
    }

    #endregion
  }
}
