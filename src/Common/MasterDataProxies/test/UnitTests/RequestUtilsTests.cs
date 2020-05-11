using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Http;
using Xunit;

namespace VSS.MasterData.Proxies.UnitTests
{
  public class RequestUtilsTests
  {
    private readonly string _precomputedTestValue = Guid.NewGuid().ToString();

    [Fact]
    public void StripHeaders_Should_return_When_receiver_is_null()
    {
      var ex = Record.Exception(() => default(Dictionary<string, string>).StripHeaders(false));

      Assert.Null(ex);
    }

    [Fact]
    public void StripHeaders_Should_retain_Internal_headers_When_already_present()
    {
      var headers = new Dictionary<string, string>
      {
        { HeaderConstants.X_VISION_LINK_CUSTOMER_UID, _precomputedTestValue }
      };

      headers.StripHeaders();

      Assert.Equal(_precomputedTestValue, headers[HeaderConstants.X_VISION_LINK_CUSTOMER_UID]);
    }

    [Theory]
    [InlineData(HeaderConstants.AUTHORIZATION)]
    [InlineData(HeaderConstants.REQUEST_ID)]
    [InlineData(HeaderConstants.X_JWT_ASSERTION)]
    [InlineData(HeaderConstants.X_REQUEST_ID)]
    [InlineData(HeaderConstants.X_VISION_LINK_CLEAR_CACHE)]
    [InlineData(HeaderConstants.X_VISION_LINK_CUSTOMER_UID)]
    [InlineData(HeaderConstants.X_VISION_LINK_USER_UID)]
    [InlineData(HeaderConstants.X_VSS_REQUEST_ID)]
    public void StripHeaders_Should_ignore_case_When_parsing_whitelist(string key)
    {
      var headers = new Dictionary<string, string>
      {
        { key.ToUpper(), _precomputedTestValue }
      };

      headers.StripHeaders();

      Assert.True(headers.ContainsKey(key.ToUpper()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("TestHeader")]
    public void StripHeaders_Internal_Should_remove_headers_When_not_whitelisted(string key)
    {
      var headers = new Dictionary<string, string>
      {
        { HeaderConstants.AUTHORIZATION, _precomputedTestValue },
        { HeaderConstants.REQUEST_ID, _precomputedTestValue },
        { HeaderConstants.X_JWT_ASSERTION, _precomputedTestValue },
        { HeaderConstants.X_REQUEST_ID, _precomputedTestValue },
        { HeaderConstants.X_VISION_LINK_CLEAR_CACHE, _precomputedTestValue },
        { HeaderConstants.X_VISION_LINK_CUSTOMER_UID, _precomputedTestValue },
        { HeaderConstants.X_VISION_LINK_USER_UID, _precomputedTestValue },
        { HeaderConstants.X_VSS_REQUEST_ID, _precomputedTestValue },
        { key, _precomputedTestValue }
      };

      headers.StripHeaders();

      Assert.False(headers.ContainsKey(key));
      Assert.True(headers.ContainsKey(HeaderConstants.AUTHORIZATION));
      Assert.True(headers.ContainsKey(HeaderConstants.REQUEST_ID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_JWT_ASSERTION));
      Assert.True(headers.ContainsKey(HeaderConstants.X_REQUEST_ID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VISION_LINK_CLEAR_CACHE));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VISION_LINK_CUSTOMER_UID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VISION_LINK_USER_UID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VSS_REQUEST_ID));
    }

    [Theory]
    [InlineData("")]
    [InlineData("TestHeader")]
    [InlineData(HeaderConstants.X_JWT_ASSERTION)]
    public void StripHeaders_External_Should_remove_headers_When_not_whitelisted(string key)
    {
      var headers = new Dictionary<string, string>
      {
        { HeaderConstants.AUTHORIZATION, _precomputedTestValue },
        { HeaderConstants.REQUEST_ID, _precomputedTestValue },
        { HeaderConstants.X_REQUEST_ID, _precomputedTestValue },
        { HeaderConstants.X_VISION_LINK_CLEAR_CACHE, _precomputedTestValue },
        { HeaderConstants.X_VISION_LINK_CUSTOMER_UID, _precomputedTestValue },
        { HeaderConstants.X_VISION_LINK_USER_UID, _precomputedTestValue },
        { HeaderConstants.X_VSS_REQUEST_ID, _precomputedTestValue },
        { key, _precomputedTestValue }
      };

      headers.StripHeaders(false);

      Assert.False(headers.ContainsKey(key));
      Assert.True(headers.ContainsKey(HeaderConstants.AUTHORIZATION));
      Assert.True(headers.ContainsKey(HeaderConstants.REQUEST_ID));
      Assert.False(headers.ContainsKey(HeaderConstants.X_JWT_ASSERTION));
      Assert.True(headers.ContainsKey(HeaderConstants.X_REQUEST_ID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VISION_LINK_CLEAR_CACHE));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VISION_LINK_CUSTOMER_UID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VISION_LINK_USER_UID));
      Assert.True(headers.ContainsKey(HeaderConstants.X_VSS_REQUEST_ID));
    }

    [Theory]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + "-")]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.AUTHORIZATION)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.REQUEST_ID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_REQUEST_ID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VISION_LINK_CLEAR_CACHE)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VISION_LINK_CUSTOMER_UID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VISION_LINK_USER_UID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VSS_REQUEST_ID)]
    public void StripHeaders_Internal_Should_accept_override_headers(string key)
    {
      var headers = new Dictionary<string, string>
      {
        { key, _precomputedTestValue }
      };

      headers.StripHeaders();

      Assert.True(headers.ContainsKey(key));
    }

    [Theory]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + "-")]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.AUTHORIZATION)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.REQUEST_ID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_REQUEST_ID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VISION_LINK_CLEAR_CACHE)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VISION_LINK_CUSTOMER_UID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VISION_LINK_USER_UID)]
    [InlineData(HeaderConstants.X_VSS_SERVICE_OVERRIDE_PREFIX + HeaderConstants.X_VSS_REQUEST_ID)]
    public void StripHeaders_External_Should_ignore_override_headers(string key)
    {
      var headers = new Dictionary<string, string>
      {
        { key, _precomputedTestValue }
      };

      headers.StripHeaders(false);

      Assert.Empty(headers);
    }
  }
}
