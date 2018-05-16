using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Validator
{
    /// <summary>
    /// Result of validation check
    /// </summary>
    public enum ValidationResult
    {
        Unknown,
        Valid,
        Invalid,
        BadRequest,
        NotProcessible
    }
}
