namespace WSCieloAPI3
{
    using System;

    public class CieloRetorno
    {
        public string MerchantOrderId;
        public WSCieloAPI3.Customer Customer;
        public WSCieloAPI3.Payment Payment;
        public CieloError Error;
        public CapturaRetorrno Captura;
    }
}

