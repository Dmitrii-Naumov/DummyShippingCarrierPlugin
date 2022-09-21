using System.Collections.Generic;

using PX.CarrierService;

namespace DummyShippingPlugin
{
    internal class DummyCarrierDetail : ICarrierDetail
    {
        public string CarrierID { get; set; }
        public string DetailID { get; set; }
        public string Descr { get; set; }
        public string Value { get; set; }

        /// <summary>
		/// Gets or sets Type of Control. Valid values are Text=1, Combo=2, Checkbox=3
		/// </summary>
        public int? ControlType { get; set; }
        private IList<KeyValuePair<string, string>> comboValues;

        public DummyCarrierDetail(string carrierID, string detailID, string descr, string value, int? controlType)
        {
            this.CarrierID = carrierID;
            this.DetailID = detailID;
            this.Descr = descr;
            this.Value = value;
            this.ControlType = controlType;

            comboValues = new List<KeyValuePair<string, string>>();
        }

        public IList<KeyValuePair<string, string>> GetComboValues()
        {
            return comboValues;
        }

        public void SetComboValues(IList<KeyValuePair<string, string>> list)
        {
            comboValues = list;
        }
    }

}
