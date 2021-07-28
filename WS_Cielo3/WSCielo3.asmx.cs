using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using WSCieloAPI3;

namespace WS_Cielo3
{
    /// <summary>
    /// Descrição resumida de WSCielo3
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // Para permitir que esse serviço da web seja chamado a partir do script, usando ASP.NET AJAX, remova os comentários da linha a seguir. 
    // [System.Web.Script.Services.ScriptService]
    public class WSCielo3 : System.Web.Services.WebService
    {

        [WebMethod]
        public string DesabilitandoRecorrencia(string chave, string MerchantOrderId)
        {
            if (this.ValidaChave(chave))
            {
                CieloAPI oapi = new CieloAPI();
                return oapi.DesabilitandoRecorrencia(MerchantOrderId);
            }
            return (-101 + "|" + "Acesso n\x00e3o autorizado");
        }

        [WebMethod]
        public string HabilitandoRecorrencia(string chave, string MerchantOrderId)
        {
            if (this.ValidaChave(chave))
            {
                CieloAPI oapi = new CieloAPI();
                return oapi.HabilitandoRecorrencia(MerchantOrderId);
            }
            return (-101 + "|" + "Acesso n\x00e3o autorizado");
        }

        [WebMethod]
        public string PagamentoRecorrente(string chave, string descricaoOperacao, string merchantOrderId, int amount, int Installments, string interval, string EndDate, string cardNumber, string Holder, string expirationDate, string securityCode, string band)
        {
            if (this.ValidaChave(chave))
            {
                CieloAPI oapi = new CieloAPI();
                CieloEnvioRecorrente dadosEnvio = new CieloEnvioRecorrente();
                CieloRetorno retorno = new CieloRetorno();
                dadosEnvio.MerchantOrderId = merchantOrderId;
                Customer customer = new Customer
                {
                    Name = descricaoOperacao
                };
                dadosEnvio.Customer = customer;
                PaymentRecurrent recurrent = new PaymentRecurrent
                {
                    Type = "CreditCard",
                    Amount = amount,
                    Installments = Installments
                };
                RecurrentPayment payment = new RecurrentPayment
                {
                    AuthorizeNow = "true",
                    Interval = interval
                };
                if (!string.IsNullOrEmpty(EndDate))
                {
                    payment.EndDate = EndDate;
                }
                recurrent.RecurrentPayment = payment;
                CreditCard card = new CreditCard
                {
                    CardNumber = cardNumber,
                    Holder = Holder,
                    ExpirationDate = expirationDate,
                    SecurityCode = securityCode,
                    Brand = band
                };
                recurrent.CreditCard = card;
                dadosEnvio.Payment = recurrent;
                retorno = oapi.RealizarPagamentoRecorrente(dadosEnvio);
                if (retorno.Error != null)
                {
                    return (retorno.Error.Code + "|" + retorno.Error.Message);
                }
                return (retorno.Captura.ReturnCode + "|" + retorno.Captura.ReturnMensagem);
            }
            return (-101 + "|" + "Acesso n\x00e3o autorizado");
        }

        [WebMethod]
        public string PagamentoRecorrenteComPeriodo(string chave, string descricaoOperacao, string merchantOrderId, int amount, int Installments, string interval, string StartDate, string EndDate, string cardNumber, string expirationDate, string holder, string securityCode, string band)
        {
            if (this.ValidaChave(chave))
            {
                CieloAPI oapi = new CieloAPI();
                CieloEnvioRecorrente dadosEnvio = new CieloEnvioRecorrente();
                CieloRetorno retorno = new CieloRetorno();
                dadosEnvio.MerchantOrderId = merchantOrderId;
                Customer customer = new Customer
                {
                    Name = descricaoOperacao
                };
                dadosEnvio.Customer = customer;
                PaymentRecurrent recurrent = new PaymentRecurrent
                {
                    Type = "CreditCard",
                    Amount = amount,
                    Installments = Installments
                };
                RecurrentPayment payment = new RecurrentPayment
                {
                    AuthorizeNow = "false",
                    Interval = interval,
                    StartDate = StartDate
                };
                if (!string.IsNullOrEmpty(EndDate))
                {
                    payment.EndDate = EndDate;
                }
                recurrent.RecurrentPayment = payment;
                CreditCard card = new CreditCard
                {
                    CardNumber = cardNumber,
                    Holder = holder,
                    ExpirationDate = expirationDate,
                    SecurityCode = securityCode,
                    Brand = band
                };
                recurrent.CreditCard = card;
                dadosEnvio.Payment = recurrent;
                retorno = oapi.RealizarPagamentoRecorrenteEspecifico(dadosEnvio);
                if (retorno.Error != null)
                {
                    return (retorno.Error.Code + "|" + retorno.Error.Message);
                }
                return (retorno.Payment.ReturnCode + "|" + retorno.Payment.ReturnMessage);
            }
            return (-101 + "|" + "Acesso n\x00e3o autorizado");
        }

        [WebMethod]
        public string PagamentoSimples(string chave, string descricaoOperacao, string merchantOrderId, int amount, int Installments, string Holder, string cardNumber, string expirationDate, string securityCode, string band)
        {
            if (this.ValidaChave(chave))
            {
                CieloAPI oapi = new CieloAPI();
                CieloEnvio dadosEnvio = new CieloEnvio();
                CieloRetorno retorno = new CieloRetorno();
                dadosEnvio.MerchantOrderId = merchantOrderId;
                Customer customer = new Customer
                {
                    Name = descricaoOperacao
                };
                dadosEnvio.Customer = customer;
                Payment payment = new Payment
                {
                    Type = "CreditCard",
                    Amount = amount,
                    Installments = Installments
                };
                CreditCard card = new CreditCard
                {
                    CardNumber = cardNumber,
                    ExpirationDate = expirationDate,
                    SecurityCode = securityCode,
                    Brand = band,
                    Holder = Holder
                };
                payment.CreditCard = card;
                dadosEnvio.Payment = payment;
                retorno = oapi.RealizarPagamentoSimples(dadosEnvio);
                if (retorno.Error != null)
                {
                    return (retorno.Error.Code + "|" + retorno.Error.Message);
                }
                return (retorno.Captura.ReturnCode + "|" + retorno.Captura.ReturnMensagem);
            }
            return (-101 + "|" + "Acesso n\x00e3o autorizado");
        }

        private bool ValidaChave(string chave)
        {
            bool flag = false;
            string str = Convert.ToString(ConfigurationManager.AppSettings["ws_chave"]);
            if (chave == str)
            {
                flag = true;
            }
            return flag;
        }
    }
}
