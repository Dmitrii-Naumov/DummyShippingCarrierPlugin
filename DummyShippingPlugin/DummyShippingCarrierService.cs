using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;

using PX.CarrierService;
using PX.Data;
using PX.SM;

using static PX.Data.BQL.BqlPlaceholder;

namespace DummyShippingPlugin
{
    [PXDisplayTypeName("DummyShippingCarrierService")]
    public class DummyShippingCarrierService : ICarrierService
    {
        #region State
        private bool active = false;
        public bool Active { get { return active; } }

        private bool logTrace = false;
        public bool LogTrace { get { return logTrace; } }

        private List<CarrierMethod> methods;

        private List<string> attributes;
        #endregion

        public DummyShippingCarrierService()
        {
            methods = new List<CarrierMethod>();
            methods.Add(GetDefaultCarrierMethod());
            methods.Add(GetAdditionalCarrierMethod());

            attributes = new List<string>(0);
        }

        public IList<string> Attributes
        {
            get { return attributes; }
        }

        public ReadOnlyCollection<CarrierMethod> AvailableMethods
        {
            get
            {
                return methods.AsReadOnly();
            }
        }

        public string CarrierID { get; set; }

        public string Method { get; set; }

        #region Interface methods

        public IList<ICarrierDetail> ExportSettings()
        {
            List<ICarrierDetail> list = new List<ICarrierDetail>();

            list.Add(new DummyCarrierDetail(CarrierID, Constants.IsActive, "Active", active.ToString(), 3));
            list.Add(new DummyCarrierDetail(CarrierID, Constants.WriteLog, "If selected, the requests and results of the web calls are saved to the trace logs.", logTrace.ToString(), 3));

            return list;
        }

        public void LoadSettings(IList<ICarrierDetail> settings)
        {
            foreach (ICarrierDetail detail in settings)
            {
                switch (detail.DetailID.ToUpper())
                {
                    case Constants.IsActive:
                        bool.TryParse(detail.Value, out bool boolValue1);
                        active = boolValue1;
                        break;
                    case Constants.WriteLog:
                        bool.TryParse(detail.Value, out bool boolValue2);
                        logTrace = boolValue2;
                        break;
                }
            }
        }

        public CarrierResult<string> Test()
        {
            try
            {
                ExecuteRequest("Test Request");
                return new CarrierResult<string>("SUCCESS");
            }
            catch (Exception ex)
            {
                return new CarrierResult<string>(false, "Error", new Message("Error", ex.Message));
            }
        }

        public CarrierResult<string> Cancel(string trackingNumber, string trackingData)
        {
            try
            {
                string message = $"Cancel request for tracking Nbr: {trackingNumber}. \r\nTracking data: {trackingData}";
                ExecuteRequest(message);
                return new CarrierResult<string>("OK");
            }
            catch (Exception ex)
            {
                return new CarrierResult<string>(false, null, new Message("Error", ex.Message));
            }
        }

        public CarrierResult<ShipResult> Return(CarrierRequest request)
        {
            try
            {
                if (request.Packages == null || request.Packages.Count == 0)
                {
                    throw new InvalidOperationException("CarrierRequest.Packages must contain at least one Package");
                }
                string message = $"Return request for method {this.Method}: \r\n";
                ExecuteRequest(message, request);

                return new CarrierResult<ShipResult>(new ShipResult(
                   GetListOfShipingMethods().First(rateQuote => rateQuote.Method.Code == this.Method)

                ));
            }

            catch (Exception ex)
            {
                return new CarrierResult<ShipResult>(false, null, new Message("Error", ex.Message));
            }
        }

        public CarrierResult<ShipResult> Ship(CarrierRequest request)
        {
            try
            {
                if (request.Packages == null || request.Packages.Count == 0)
                {
                    throw new InvalidOperationException("CarrierRequest.Packages must contain at least one Package");
                }
                string message = $"Ship request for method {this.Method}: \r\n";
                ExecuteRequest(message, request);

                var result = new CarrierResult<ShipResult>(new ShipResult(
                     GetListOfShipingMethods().First(rateQuote => rateQuote.Method.Code == this.Method)

                 ));
                int trackingNbr = 12345;
                string shipmentNbr =
                    request.Attributes.Where(_ => _.StartsWith("ShipmentNbr:")).FirstOrDefault()
                    ?? DateTime.Now.ToString();
                foreach (var package in request.Packages)
                {
                    trackingNbr++;
                    result.Result.Data.Add(new PackageData(package.RefNbr, shipmentNbr+ trackingNbr.ToString(), ReadResource("Label.pdf"), "pdf"));
                    result.Result.Data.Add(new PackageData(package.RefNbr, shipmentNbr+ trackingNbr.ToString() + "c", ReadResource("Customs Declaration.pdf"), "pdf"));
                }
                return result;
            }

            catch (Exception ex)
            {
                return new CarrierResult<ShipResult>(false, null, new Message("Error", ex.Message));
            }
        }
        public CarrierResult<RateQuote> GetRateQuote(CarrierRequest request)
        {
            try
            {
                string message = $"GetRateQuote request for method {this.Method}: \r\n";
                ExecuteRequest(message, request);

                return new CarrierResult<RateQuote>(
                    GetListOfShipingMethods().First(rateQuote => rateQuote.Method.Code == this.Method)
                );
            }

            catch (Exception ex)
            {
                return new CarrierResult<RateQuote>(false, null, new Message("Error", ex.Message));
            }
        }

        public CarrierResult<IList<RateQuote>> GetRateList(CarrierRequest request)
        {
            try
            {
                string message = "GetRateQuote request: \r\n";
                ExecuteRequest(message, request);

                return new CarrierResult<IList<RateQuote>>(
                        GetListOfShipingMethods()
                    );
            }

            catch (Exception ex)
            {
                return new CarrierResult<IList<RateQuote>>(false, null, new Message("Error", ex.Message));
            }
        }


        public CarrierResult<IList<CarrierCertificationData>> GetCertificationData()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Auxiliary
        private void ExecuteRequest(string message, object requestData = null)
        {
            if (!Active)
            {
                throw new Exception("The service is not active");
            }
            else if (LogTrace)
            {
                if (requestData == null)
                {
                    PXTrace.WriteInformation(message);
                }
                else
                {
                    PXTrace.WriteInformation(message + JsonConvert.SerializeObject(requestData, Formatting.Indented));
                }
            }
        }

        private static List<RateQuote> GetListOfShipingMethods()
        {
            return new List<RateQuote>()
                    {
                        new RateQuote(
                            "USD",
                            10,
                            GetDefaultCarrierMethod(),
                            DateTime.Now.AddDays(2)),
                        new RateQuote(
                            "USD",
                            5,
                            GetAdditionalCarrierMethod(),
                            DateTime.Now.AddDays(5))
                    };
        }

        private static CarrierMethod GetDefaultCarrierMethod()
        {
            return new CarrierMethod(Constants.MethodCode, Constants.MethodDescription);
        }

        private static CarrierMethod GetAdditionalCarrierMethod()
        {
            return new CarrierMethod(Constants.AdditionalMethodCode, Constants.AdditionalMethodDescription);
        }
        #endregion
        #region Helper
        public byte[] ReadResource(string name)
        {
            // Determine path
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;

            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return reader.ReadBytes((int)stream.Length);
            }
        }
        #endregion
    }
}