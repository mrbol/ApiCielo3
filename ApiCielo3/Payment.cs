namespace WSCieloAPI3
{
    using System;
    using System.Collections.Generic;

    public class Payment
    {
        public int ServiceTaxAmount;
        public int Installments;
        public int Interest;
        public bool Capture;
        public bool Authenticate;
        public bool Recurrent;
        public WSCieloAPI3.RecurrentPayment RecurrentPayment;
        public WSCieloAPI3.CreditCard CreditCard;
        public string Tid;
        public string ProofOfSale;
        public string AuthorizationCode;
        public string SoftDescriptor;
        public string Provider;
        public string PaymentId;
        public string Type;
        public int Amount;
        public string ReceivedDate;
        public string Currency;
        public string Country;
        public string ReturnCode;
        public string ReturnMessage;
        public int Status;
        public List<Link> Links;
    }
}

