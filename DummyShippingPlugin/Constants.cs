using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyShippingPlugin
{
    internal static class Constants
    {
        #region DetailIDs const

        public const string IsActive = "ISACTIVE";
        public const string WriteLog = "WRITELOG";

        #endregion

        public const string MethodCode = "2DaysAir";
        public const string MethodDescription = "Delivery in 2 days by a plane";
        public const string AdditionalMethodCode = "5BDaysGroud";
        public const string AdditionalMethodDescription = "Delivery in 5 days by a truck";
    }
}
