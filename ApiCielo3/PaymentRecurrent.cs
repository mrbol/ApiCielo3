namespace WSCieloAPI3
{
    using System;

    public class PaymentRecurrent
    {
        public string Type;
        public int Amount;
        public int Installments;
        public string SoftDescriptor;
        public WSCieloAPI3.RecurrentPayment RecurrentPayment;
        public WSCieloAPI3.CreditCard CreditCard;
    }
}

