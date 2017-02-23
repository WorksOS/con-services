
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Validation;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.ResultHandling;


namespace VSS.Raptor.Service.Common.Models
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
        /// ProjectID sample instance.
        /// </summary>
        /// 
        public static DataID HelpSample
        {
          get { return new DataID() { dataId = 1 }; }
        }

        /// <summary>
        /// Creates an instance of the ProjectID class.
        /// </summary>
        /// <param name="dataId">The data identifier.</param>
        /// <returns></returns>
        /// 
        public static DataID CreateDataID(long dataId)       
        {
          return new DataID() { dataId = dataId };
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
