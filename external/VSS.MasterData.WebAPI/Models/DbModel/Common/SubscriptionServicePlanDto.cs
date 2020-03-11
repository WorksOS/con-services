using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.AssetSettings
{
	public class SubscriptionServicePlanDto
	{
		private string _serviceUIDString;
		private string _assetUIDString;
		private string _customerUIDString;
		private string _deviceUIDString;

		public int ServiceTypeID { get; set; }
		public Guid ServiceUID
		{
			get { return Guid.Parse(_serviceUIDString); }
			set { _serviceUIDString = value.ToString("N"); }
		}
		public string ServiceUIDString
		{
			get { return this._serviceUIDString.ToLower(); }
			set { this._serviceUIDString = value; }
		}
		public Guid CustomerUID
		{
			get { return Guid.Parse(_customerUIDString); }
			set { _customerUIDString = value.ToString("N"); }
		}
		public string CustomerUIDString
		{
			get { return this._customerUIDString.ToLower(); }
			set { this._customerUIDString = value; }
		}
		public Guid AssetUID
		{
			get { return Guid.Parse(_assetUIDString); }
			set { _assetUIDString = value.ToString("N"); }
		}
		public string AssetUIDString
		{
			get { return this._assetUIDString.ToLower(); }
			set { this._assetUIDString = value; }
		}
		public Guid DeviceUID
		{
			get { return Guid.Parse(_deviceUIDString); }
			set { _deviceUIDString = value.ToString("N"); }
		}
		public string DeviceUIDString
		{
			get { return this._deviceUIDString.ToLower(); }
			set { this._deviceUIDString = value; }
		}
		public int SubscriptionSourceID { get; set; }
		public string PlanName { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime RowUpdatedUTC { get; set; }
	}
}
