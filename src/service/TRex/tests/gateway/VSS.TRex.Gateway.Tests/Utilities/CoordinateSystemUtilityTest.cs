﻿using FluentAssertions;
using VSS.Common.Exceptions;
using VSS.TRex.Gateway.Common.Utilities;
using Xunit;

namespace VSS.TRex.Gateway.Tests.Utilities
{
  public class CoordinateSystemUtilityTest
  {
    private const string NO_CSIB_KEY = "-1";
    private const string CSIB_KEY = "GR0G000ZHC4000000000800BY7SN2W0EYST640036P3P1SV09C1G61CZZKJC976CNB295K7W7G30DA30A1N74ZJH1831E5V0CHJ60W295GMWT3E95154T3A85H5CRK9D94PJM1P9Q6R30E1C1E4Q173W9XDE923XGGHN8JR37B6RESPQ3ZHWW6YV5PFDGCTZYPWDSJEFE1G2THV3VAZVN28ECXY7ZNBYANFEG452TZZ3X2Q1GCYM8EWCRVGKWD5KANKTXA1MV0YWKRBKBAZYVXXJRM70WKCN2X1CX96TVXKFRW92YJBT5ZCFSVM37ZD5HKVFYYYMJVS45K4PT1TGKYDDZ1SKZ176EJ0F5VB1FH34C5J68S36CZ5E001G1A4H39ERC08000";

    [Fact]
    public void ConvertCSIBKeyToString_No_Key()
    {
      CoordinateSystemUtility.FromCSIBKeyToString(NO_CSIB_KEY).Should().Be(string.Empty);
    }

    [Fact]
    public void ConvertCSIBKeyToString_With_Key()
    {;
      var resultCSIB = "VE5MIENTSUIAAAAAAAAmQFNDUzkwMCBMb2NhbGl6YXRpb24AAFNDUzkwMCBSZWNvcmQAAFNDUzkwMCBSZWNvcmQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEBAABEYXR1bVRocmVlUGFyYW1ldGVycwAAAABEBABAplRYQZPeGxTEP1hBAAAAAAAAAIAAAAAAAAAAgAAAAAAAAACADlNDUzkwMCBSZWNvcmQAAPwzidi3OOQ/A9VI04kPAMCtCIFm/n6RQMxN+n31I6FAVC6H71oA8D8AAAAAAADwPwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABU0NTOTAwIFJlY29yZAAAJZmoeTC7kkDzZo8993ajQECCedBUzGU/2wH4yFX6IT/4iVvMj3UBP/+MJqUNAPA/AAAAGC1EVPsh+b8YLURU+yEJQBgtRFT7Ifk/GC1EVPshCUABAQQBAQEDAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

      CoordinateSystemUtility.FromCSIBKeyToString(CSIB_KEY).Should().Be(resultCSIB);
    }

    [Fact]
    public void ConvertCSIBKeyToBytes_No_Key()
    {
      CoordinateSystemUtility.FromCSIBKeyToBytes(NO_CSIB_KEY).Should().BeNull();
    }

    [Fact]
    public void ConvertCSIBKeyToBytes_With_Key()
    {
      CoordinateSystemUtility.FromCSIBKeyToBytes(CSIB_KEY).Should().NotBeNull();
    }
  }
}