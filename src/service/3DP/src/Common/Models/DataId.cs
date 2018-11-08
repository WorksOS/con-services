using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Raptor filter identifier.
  /// </summary>
  ///
  public class DataID : IValidatable
    {
        /// <summary>
        /// The ID of a filter.
        /// </summary>
        /// 
        [JsonProperty(PropertyName = "dataId", Required = Required.Always)]
        [Required]
        [ValidFilterID]
        public long dataId { get; private set; }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// 
        private DataID()
        {
            // ...
        }
    
        /// <summary>
        /// Creates an instance of the ProjectID class.
        /// </summary>
        /// <param name="dataId">The data identifier.</param>
        /// <returns></returns>
        /// 
        public static DataID CreateDataID(long dataId)       
        {
          return new DataID { dataId = dataId };
        }

        /// <summary>
        /// Validation method.
        /// </summary>
        public void Validate()
        {
            // Validation rules might be placed in here...
            // throw new NotImplementedException();
            var validator = new DataAnnotationsValidator();
            ICollection<ValidationResult> results;
            validator.TryValidate(this, out results);
            if (results.Any())
            {
                throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
            }
        }

    }
}
