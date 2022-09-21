using System;
using System.Collections.Generic;

using PX.CarrierService;
using PX.Data;
using PX.Objects.SO;

using Document = PX.Objects.SO.GraphExtensions.CarrierRates.Document;

namespace ShippingExtension
{
    public class Dummy_SOShipmentEntryExtension : PXGraphExtension<SOShipmentEntry.CarrierRates, SOShipmentEntry>
    {
        public static bool IsActive()
        {
            return true;
        }

        [PXOverride]
        public virtual CarrierRequest GetCarrierRequest(
            Document doc,
            UnitsType unit,
            List<string> methods,
            List<CarrierBoxEx> boxes,
            Func<Document, UnitsType, List<string>, List<CarrierBoxEx>, CarrierRequest> baseMethod)
        {
            return baseMethod(doc, unit, methods, boxes);
        }
        [PXOverride]
        public virtual CarrierRequest BuildRequest(SOShipment shiporder, Func<SOShipment, CarrierRequest> baseMethod)
        {
            var result = baseMethod(shiporder);
            result.Attributes.Add($"ShipmentNbr: {shiporder.ShipmentNbr}");
            return result;
        }
    }
}
