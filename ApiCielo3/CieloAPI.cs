
using Microsoft.Practices.EnterpriseLibrary.Data;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using System.Configuration;

namespace WSCieloAPI3
{
    public class CieloAPI
    {
        private string _merchantKey = ConfigurationManager.AppSettings["MerchantKey"];
        private string _merchantId = ConfigurationManager.AppSettings["MerchantId"];
        private string _urlCielo = ConfigurationManager.AppSettings["URL_CIELO_API_3"];
        private CieloEnvio _dadosEnvioCielo;
        private CieloEnvioRecorrente _dadosEnvioCieloRecorrente;
        private CieloRetorno _dadosRetornoCielo = new CieloRetorno();
        private string _jsonRetornoCielo = string.Empty;
        private CaptureEnvio _dadosCapturaEnvio;
        private CapturaRetorrno _dadosCaptureRetorno;
        private string _jsonRetornoCaptura = string.Empty;
        private CieloError _cieloError = new CieloError();
        private string _jsonRetornoCieloError = string.Empty;

        private bool CaptureOperacao(EnumTipoOperacao tipo, EnumMetodos metodo, CaptureEnvio dadosCaptureEnvio)
        {
            bool flag = true;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(dadosCaptureEnvio.UrlBase);
                RestRequest request = new RestRequest(Method.PUT);
                request.AddParameter("amount", dadosCaptureEnvio.Amount);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Accept-Encoding", "gzip");
                request.AddHeader("MerchantId", this._merchantId);
                request.AddHeader("MerchantKey", this._merchantKey);
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);
                string content = response.Content;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    this._jsonRetornoCaptura = response.Content;
                    Dictionary<string, object> dictionary = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(response.Content);
                    this._dadosCaptureRetorno = new CapturaRetorrno();
                    foreach (KeyValuePair<string, object> pair in dictionary)
                    {
                        if (pair.Key == "Status")
                        {
                            this._dadosCaptureRetorno.Status = (int)pair.Value;
                        }
                        else if (pair.Key == "ReturnCode")
                        {
                            this._dadosCaptureRetorno.ReturnCode = (string)pair.Value;
                        }
                        else if (pair.Key == "ReturnMessage")
                        {
                            this._dadosCaptureRetorno.ReturnMensagem = (string)pair.Value;
                        }
                    }
                    this._dadosRetornoCielo.Captura = this._dadosCaptureRetorno;
                    this._dadosRetornoCielo.Payment.Capture = true;
                    return flag;
                }
                flag = false;
                this._jsonRetornoCieloError = response.Content;
                Dictionary<string, object> dictionary2 = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(response.Content);
                foreach (KeyValuePair<string, object> pair2 in dictionary2)
                {
                    if (pair2.Key == "Code")
                    {
                        this._cieloError.Code = (int)pair2.Value;
                    }
                    else if (pair2.Key == "Message")
                    {
                        this._cieloError.Message = (string)pair2.Value;
                    }
                }
                return flag;
            }
            catch (Exception exception)
            {
                flag = false;
                this._cieloError.Code = -100;
                this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
            }
            finally
            {
                this.RegistraRetornoCielo(tipo, metodo);
            }
            return flag;
        }

        private string ControlaRecorrencia(EnumTipoOperacao tipo, EnumMetodos metodo, string urlBase, string MerchantOrderId)
        {
            string str = string.Empty;
            try
            {
                string str2 = (tipo == EnumTipoOperacao.DesabilitarRecorrencia) ? "Deactivate" : "Reactivate";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(urlBase.Replace("https://apiquery.cieloecommerce.cielo.com.br/", "https://api.cieloecommerce.cielo.com.br/"));
                RestRequest request = new RestRequest("/{id}", Method.PUT);
                request.AddUrlSegment("id", str2);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Accept-Encoding", "gzip");
                request.AddHeader("MerchantId", this._merchantId);
                request.AddHeader("MerchantKey", this._merchantKey);
                request.RequestFormat = DataFormat.Json;
                IRestResponse response = client.Execute(request);
                string content = response.Content;
                this._jsonRetornoCielo = response.Content;
                str = Convert.ToInt32(response.StatusCode) + "|" + response.StatusDescription;
            }
            catch (Exception exception)
            {
                this._cieloError.Code = -100;
                this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
            }
            finally
            {
                this._dadosEnvioCielo = new CieloEnvio();
                this._dadosEnvioCielo.Customer = new Customer();
                this._dadosEnvioCielo.Customer.Name = Convert.ToString(tipo) + " - " + Convert.ToString(metodo);
                this._dadosEnvioCielo.MerchantOrderId = MerchantOrderId;
                this.RegistraRetornoCielo(EnumTipoOperacao.DesabilitarRecorrencia, EnumMetodos.DesabilitarRecorrencia);
            }
            return str;
        }

        public string DesabilitandoRecorrencia(string MerchantOrderId)
        {
            string link = string.Empty;
            string str2 = string.Empty;
            if (this.RecuperaLinkRecorrencia(MerchantOrderId, out link))
            {
                str2 = this.ControlaRecorrencia(EnumTipoOperacao.DesabilitarRecorrencia, EnumMetodos.DesabilitarRecorrencia, link, MerchantOrderId);
            }
            return str2;
        }

        public string HabilitandoRecorrencia(string MerchantOrderId)
        {
            string link = string.Empty;
            string str2 = string.Empty;
            if (this.RecuperaLinkRecorrencia(MerchantOrderId, out link))
            {
                str2 = this.ControlaRecorrencia(EnumTipoOperacao.HabilitarRecorrencia, EnumMetodos.HabilitarRecorrencia, link, MerchantOrderId);
            }
            return str2;
        }

        public CieloRetorno RealizarPagamentoRecorrente(CieloEnvioRecorrente dadosEnvio)
        {
            new CieloRetorno();
            if (this.ValidaDadosIntegracaoRecorrente(dadosEnvio))
            {
                try
                {
                    this._dadosEnvioCieloRecorrente = dadosEnvio;
                    if (this.SolicitaAutorizacaoRecorrente())
                    {
                        if ((this._dadosRetornoCielo.Payment.ReturnCode == "00") || (this._dadosRetornoCielo.Payment.ReturnCode == "000"))
                        {
                            CaptureEnvio dadosCaptureEnvio = new CaptureEnvio
                            {
                                PaymentId = this._dadosRetornoCielo.Payment.PaymentId,
                                Amount = this._dadosRetornoCielo.Payment.Amount,
                                UrlBase = this._dadosRetornoCielo.Payment.Links[0].Href
                            };
                            this.CaptureOperacao(EnumTipoOperacao.Recorrente, EnumMetodos.Captura, dadosCaptureEnvio);
                        }
                        else
                        {
                            this._cieloError.Code = Convert.ToInt32(this._dadosRetornoCielo.Payment.ReturnCode);
                            this._cieloError.Message = this._dadosRetornoCielo.Payment.ReturnMessage;
                        }
                    }
                }
                catch (Exception exception)
                {
                    this._cieloError.Code = -100;
                    this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
                }
            }
            return this.TrataResposta();
        }

        public CieloRetorno RealizarPagamentoRecorrenteEspecifico(CieloEnvioRecorrente dadosEnvio)
        {
            new CieloRetorno();
            if (this.ValidaDadosIntegracaoRecorrenteEspecifico(dadosEnvio))
            {
                try
                {
                    this._dadosEnvioCieloRecorrente = dadosEnvio;
                    this.SolicitaAutorizacaoRecorrente();
                }
                catch (Exception exception)
                {
                    this._cieloError.Code = -100;
                    this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
                }
            }
            return this.TrataResposta();
        }

        public CieloRetorno RealizarPagamentoSimples(CieloEnvio dadosEnvio)
        {
            new CieloRetorno();
            if (this.ValidaDadosIntegracao(dadosEnvio, EnumTipoOperacao.Simples))
            {
                try
                {
                    this._dadosEnvioCielo = dadosEnvio;
                    if (this.SolicitaAutorizacao())
                    {
                        if ((this._dadosRetornoCielo.Payment.ReturnCode == "00") || (this._dadosRetornoCielo.Payment.ReturnCode == "000"))
                        {
                            CaptureEnvio dadosCaptureEnvio = new CaptureEnvio
                            {
                                PaymentId = this._dadosRetornoCielo.Payment.PaymentId,
                                Amount = this._dadosRetornoCielo.Payment.Amount,
                                UrlBase = (this._dadosRetornoCielo.Payment.Links.Count<Link>() > 0) ? this._dadosRetornoCielo.Payment.Links[0].Href : string.Empty
                            };
                            if (!string.IsNullOrEmpty(dadosCaptureEnvio.UrlBase))
                            {
                                this.CaptureOperacao(EnumTipoOperacao.Simples, EnumMetodos.Captura, dadosCaptureEnvio);
                            }
                            else
                            {
                                this._cieloError.Code = -100;
                                this._cieloError.Message = string.Concat(new object[] { "CAPTURA N\x00c3O REALIZADA - PaymentId ", dadosCaptureEnvio.PaymentId, " Amount ", dadosCaptureEnvio.Amount, " urlbase ", dadosCaptureEnvio.UrlBase });
                            }
                        }
                        else
                        {
                            this._cieloError.Code = Convert.ToInt32(this._dadosRetornoCielo.Payment.ReturnCode);
                            this._cieloError.Message = this._dadosRetornoCielo.Payment.ReturnMessage;
                        }
                    }
                }
                catch (Exception exception)
                {
                    this._cieloError.Code = -100;
                    this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
                }
            }
            return this.TrataResposta();
        }

        private bool RecuperaLinkRecorrencia(string MerchantOrderId, out string link)
        {
            bool flag = true;
            string str = string.Empty;
            //try
            //{
            //    Database database = DatabaseFactory.CreateDatabase();
            //    string query = "spws_RetornaLinkRecorrencia";
            //    DbCommand sqlStringCommand = database.GetSqlStringCommand(query);
            //    sqlStringCommand.CommandType = CommandType.StoredProcedure;
            //    database.AddInParameter(sqlStringCommand, "@idpedido", DbType.String, MerchantOrderId);
            //    str = Convert.ToString(database.ExecuteScalar(sqlStringCommand));
            //    if (!string.IsNullOrEmpty(str))
            //    {
            //        Dictionary<string, object> dictionary = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(str);
            //        foreach (KeyValuePair<string, object> pair in dictionary)
            //        {
            //            if (pair.Key == "Payment")
            //            {
            //                foreach (KeyValuePair<string, object> pair2 in (Dictionary<string, object>)pair.Value)
            //                {
            //                    if (pair2.Key == "RecurrentPayment")
            //                    {
            //                        foreach (KeyValuePair<string, object> pair3 in (Dictionary<string, object>)pair2.Value)
            //                        {
            //                            if (pair3.Key == "Link")
            //                            {
            //                                foreach (KeyValuePair<string, object> pair4 in (Dictionary<string, object>)pair3.Value)
            //                                {
            //                                    if (pair4.Key == "Href")
            //                                    {
            //                                        str = (string)pair4.Value;
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch
            //{
            //    flag = false;
            //}
            link = str;
            return flag;
        }

        private void RegistrarEnvio(string conteudo)
        {
            try
            {
                #region Log de envio 
                //Database database = DatabaseFactory.CreateDatabase();
                //string query = "sp_EnvioDados";
                //DbCommand sqlStringCommand = database.GetSqlStringCommand(query);
                //sqlStringCommand.CommandType = CommandType.StoredProcedure;
                //database.AddInParameter(sqlStringCommand, "@conteudo", DbType.String, conteudo);
                //database.ExecuteNonQuery(sqlStringCommand);
                #endregion
            }
            catch
            {
            }
        }

        private void RegistraRetornoCielo(EnumTipoOperacao tipo, EnumMetodos metodo)
        {
            //utilizar registrar retorno da ciela para um aqui de Log

            #region utilizar registrar retorno da ciela para BD
            //try
            //{
            //    Database database = DatabaseFactory.CreateDatabase();
            //    string query = "spws_RegistraLogCielo";
            //    DbCommand sqlStringCommand = database.GetSqlStringCommand(query);
            //    sqlStringCommand.CommandType = CommandType.StoredProcedure;
            //    database.AddInParameter(sqlStringCommand, "@acao", DbType.String, Convert.ToString(tipo));
            //    database.AddInParameter(sqlStringCommand, "@metodo", DbType.String, Convert.ToString(metodo));
            //    if (this._dadosEnvioCielo != null)
            //    {
            //        database.AddInParameter(sqlStringCommand, "@IdPedido", DbType.String, this._dadosEnvioCielo.MerchantOrderId);
            //        database.AddInParameter(sqlStringCommand, "@Titulo", DbType.String, this._dadosEnvioCielo.Customer.Name);
            //    }
            //    else
            //    {
            //        database.AddInParameter(sqlStringCommand, "@IdPedido", DbType.String, this._dadosEnvioCieloRecorrente.MerchantOrderId);
            //        database.AddInParameter(sqlStringCommand, "@Titulo", DbType.String, this._dadosEnvioCieloRecorrente.Customer.Name);
            //    }
            //    if (string.IsNullOrEmpty(this._cieloError.Message))
            //    {
            //        if (EnumMetodos.Captura == metodo)
            //        {
            //            database.AddInParameter(sqlStringCommand, "@Conteudo", DbType.String, this._jsonRetornoCaptura);
            //        }
            //        else
            //        {
            //            database.AddInParameter(sqlStringCommand, "@Conteudo", DbType.String, this._jsonRetornoCielo);
            //        }
            //    }
            //    else
            //    {
            //        database.AddInParameter(sqlStringCommand, "@Conteudo", DbType.String, this._jsonRetornoCieloError);
            //    }
            //    database.ExecuteNonQuery(sqlStringCommand);
            //}
            //catch
            //{
            //}
            #endregion
        }

        private bool SolicitaAutorizacao()
        {
            bool flag = true;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(this._urlCielo);
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Accept-Encoding", "gzip");
                request.AddHeader("MerchantId", this._merchantId);
                request.AddHeader("MerchantKey", this._merchantKey);
                string conteudo = new JavaScriptSerializer().Serialize(this._dadosEnvioCielo);
                this.RegistrarEnvio(conteudo);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(this._dadosEnvioCielo);
                IRestResponse response = client.Execute(request);
                string content = response.Content;
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    this._jsonRetornoCielo = response.Content;
                    Dictionary<string, object> dictionary = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(response.Content);
                    CieloRetorno retorno = new CieloRetorno();
                    foreach (KeyValuePair<string, object> pair in dictionary)
                    {
                        if (pair.Key == "MerchantOrderId")
                        {
                            retorno.MerchantOrderId = (string)pair.Value;
                        }
                        if (pair.Key == "Customer")
                        {
                            retorno.Customer = new Customer();
                            foreach (KeyValuePair<string, object> pair2 in (Dictionary<string, object>)pair.Value)
                            {
                                if (pair2.Key == "Name")
                                {
                                    retorno.Customer.Name = (string)pair2.Value;
                                }
                            }
                        }
                        if (pair.Key == "Payment")
                        {
                            retorno.Payment = new Payment();
                            foreach (KeyValuePair<string, object> pair3 in (Dictionary<string, object>)pair.Value)
                            {
                                if (pair3.Key == "PaymentId")
                                {
                                    retorno.Payment.PaymentId = (string)pair3.Value;
                                }
                                if (pair3.Key == "Amount")
                                {
                                    retorno.Payment.Amount = (int)pair3.Value;
                                }
                                if (pair3.Key == "ReturnCode")
                                {
                                    retorno.Payment.ReturnCode = (string)pair3.Value;
                                }
                                if (pair3.Key == "ReturnMessage")
                                {
                                    retorno.Payment.ReturnMessage = (string)pair3.Value;
                                }
                                if (pair3.Key == "Links")
                                {
                                    retorno.Payment.Links = new List<Link>();
                                    object[] objArray = (object[])pair3.Value;
                                    Link item = new Link();
                                    if (((objArray != null) && (objArray.Count<object>() > 0)) && (objArray.Count<object>() > 1))
                                    {
                                        foreach (KeyValuePair<string, object> pair4 in (Dictionary<string, object>)objArray[1])
                                        {
                                            if (pair4.Key == "Method")
                                            {
                                                item.Method = (string)pair4.Value;
                                            }
                                            else if (pair4.Key == "Rel")
                                            {
                                                item.Rel = (string)pair4.Value;
                                            }
                                            else if (pair4.Key == "Href")
                                            {
                                                item.Href = (string)pair4.Value;
                                            }
                                        }
                                        retorno.Payment.Links.Add(item);
                                    }
                                }
                            }
                        }
                    }
                    this._dadosRetornoCielo = retorno;
                    return flag;
                }
                flag = false;
                this._jsonRetornoCieloError = response.Content;
                object[] source = (object[])new JavaScriptSerializer().DeserializeObject(response.Content);
                if ((source != null) && (source.Count<object>() > 0))
                {
                    foreach (KeyValuePair<string, object> pair5 in (Dictionary<string, object>)source[0])
                    {
                        if (pair5.Key == "Code")
                        {
                            this._cieloError.Code = (int)pair5.Value;
                        }
                        else if (pair5.Key == "Message")
                        {
                            this._cieloError.Message = (string)pair5.Value;
                        }
                    }
                    return flag;
                }
                this._cieloError.Code = Convert.ToInt32(response.ResponseStatus);
                this._cieloError.Message = Convert.ToString(response.StatusDescription);
                return flag;
            }
            catch (Exception exception)
            {
                flag = false;
                this._cieloError.Code = -100;
                this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
            }
            finally
            {
                this.RegistraRetornoCielo(EnumTipoOperacao.Simples, EnumMetodos.Autorizao);
            }
            return flag;
        }

        private bool SolicitaAutorizacaoRecorrente()
        {
            bool flag = true;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(this._urlCielo);
                RestRequest request = new RestRequest(Method.POST);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Accept-Encoding", "gzip");
                request.AddHeader("MerchantId", this._merchantId);
                request.AddHeader("MerchantKey", this._merchantKey);
                request.RequestFormat = DataFormat.Json;
                Dictionary<string, object> dictionary = new Dictionary<string, object> {
                    {
                        "MerchantOrderId",
                        this._dadosEnvioCieloRecorrente.MerchantOrderId
                    },
                    {
                        "Customer",
                        this._dadosEnvioCieloRecorrente.Customer
                    },
                    {
                        "Payment",
                        this._dadosEnvioCieloRecorrente.Payment
                    }
                };
                new JavaScriptSerializer().Serialize(dictionary);
                request.AddBody(dictionary);
                IRestResponse response = client.Execute(request);
                string content = response.Content;
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    this._jsonRetornoCielo = response.Content;
                    Dictionary<string, object> dictionary2 = (Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(response.Content);
                    CieloRetorno retorno = new CieloRetorno();
                    foreach (KeyValuePair<string, object> pair in dictionary2)
                    {
                        if (pair.Key == "MerchantOrderId")
                        {
                            retorno.MerchantOrderId = (string)pair.Value;
                        }
                        if (pair.Key == "Customer")
                        {
                            retorno.Customer = new Customer();
                            foreach (KeyValuePair<string, object> pair2 in (Dictionary<string, object>)pair.Value)
                            {
                                if (pair2.Key == "Name")
                                {
                                    retorno.Customer.Name = (string)pair2.Value;
                                }
                            }
                        }
                        if (pair.Key == "Payment")
                        {
                            retorno.Payment = new Payment();
                            foreach (KeyValuePair<string, object> pair3 in (Dictionary<string, object>)pair.Value)
                            {
                                if (pair3.Key == "PaymentId")
                                {
                                    retorno.Payment.PaymentId = (string)pair3.Value;
                                }
                                if (pair3.Key == "Amount")
                                {
                                    retorno.Payment.Amount = (int)pair3.Value;
                                }
                                if (pair3.Key == "ReturnCode")
                                {
                                    retorno.Payment.ReturnCode = (string)pair3.Value;
                                }
                                if (pair3.Key == "ReturnMessage")
                                {
                                    retorno.Payment.ReturnMessage = (string)pair3.Value;
                                }
                                if (pair3.Key == "Links")
                                {
                                    retorno.Payment.Links = new List<Link>();
                                    object[] objArray = (object[])pair3.Value;
                                    Link item = new Link();
                                    if (((objArray != null) && (objArray.Count<object>() > 0)) && (objArray.Count<object>() > 1))
                                    {
                                        foreach (KeyValuePair<string, object> pair4 in (Dictionary<string, object>)objArray[1])
                                        {
                                            if (pair4.Key == "Method")
                                            {
                                                item.Method = (string)pair4.Value;
                                            }
                                            else if (pair4.Key == "Rel")
                                            {
                                                item.Rel = (string)pair4.Value;
                                            }
                                            else if (pair4.Key == "Href")
                                            {
                                                item.Href = (string)pair4.Value;
                                            }
                                            retorno.Payment.Links.Add(item);
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(retorno.Payment.ReturnMessage) && (pair3.Key == "RecurrentPayment"))
                                {
                                    foreach (KeyValuePair<string, object> pair5 in (Dictionary<string, object>)pair3.Value)
                                    {
                                        if (pair5.Key == "ReasonMessage")
                                        {
                                            retorno.Payment.ReturnCode = "0";
                                            retorno.Payment.ReturnMessage = (string)pair5.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    this._dadosRetornoCielo = retorno;
                    return flag;
                }
                flag = false;
                this._jsonRetornoCieloError = response.Content;
                object[] source = (object[])new JavaScriptSerializer().DeserializeObject(response.Content);
                if ((source != null) && (source.Count<object>() > 0))
                {
                    foreach (KeyValuePair<string, object> pair6 in (Dictionary<string, object>)source[0])
                    {
                        if (pair6.Key == "Code")
                        {
                            this._cieloError.Code = (int)pair6.Value;
                        }
                        else if (pair6.Key == "Message")
                        {
                            this._cieloError.Message = (string)pair6.Value;
                        }
                    }
                    return flag;
                }
                this._cieloError.Code = Convert.ToInt32(response.ResponseStatus);
                this._cieloError.Message = Convert.ToString(response.StatusDescription);
                return flag;
            }
            catch (Exception exception)
            {
                flag = false;
                this._cieloError.Code = -100;
                this._cieloError.Message = this._cieloError.Message + " - " + exception.Message + ((exception.InnerException != null) ? exception.InnerException.Message : string.Empty);
            }
            finally
            {
                this.RegistraRetornoCielo(EnumTipoOperacao.Recorrente, EnumMetodos.Recorrente);
            }
            return flag;
        }

        private CieloRetorno TrataResposta()
        {
            CieloRetorno retorno = new CieloRetorno();
            if (!string.IsNullOrEmpty(this._cieloError.Message))
            {
                retorno.Error = this._cieloError;
                return retorno;
            }
            return this._dadosRetornoCielo;
        }

        private bool ValidaDadosIntegracao(CieloEnvio obj, EnumTipoOperacao tipo)
        {
            bool flag = true;
            switch (tipo)
            {
                case EnumTipoOperacao.Simples:
                    if (!string.IsNullOrEmpty(obj.MerchantOrderId))
                    {
                        if (string.IsNullOrEmpty(obj.Payment.Type))
                        {
                            flag = false;
                            this._cieloError.Code = -2;
                            this._cieloError.Message = "O Payment.Type n\x00e3o foi informado";
                            return flag;
                        }
                        if (Convert.ToInt32(obj.Payment.Amount) == 0)
                        {
                            flag = false;
                            this._cieloError.Code = -3;
                            this._cieloError.Message = "O Amount n\x00e3o foi informado";
                            return flag;
                        }
                        if (Convert.ToInt32(obj.Payment.Installments) == 0)
                        {
                            flag = false;
                            this._cieloError.Code = -1;
                            this._cieloError.Message = "O Installments n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.CardNumber))
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O CardNumber n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.ExpirationDate))
                        {
                            flag = false;
                            this._cieloError.Code = -6;
                            this._cieloError.Message = "O ExpirationDate n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.SecurityCode))
                        {
                            flag = false;
                            this._cieloError.Code = -7;
                            this._cieloError.Message = "O SecurityCode n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.Brand))
                        {
                            flag = false;
                            this._cieloError.Code = -8;
                            this._cieloError.Message = "O Brand n\x00e3o foi informado";
                        }
                        return flag;
                    }
                    flag = false;
                    this._cieloError.Code = -1;
                    this._cieloError.Message = "O MerchantOrderId n\x00e3o foi informado";
                    return flag;

                case EnumTipoOperacao.Recorrente:
                    if (!string.IsNullOrEmpty(obj.MerchantOrderId))
                    {
                        if (string.IsNullOrEmpty(obj.Payment.Type))
                        {
                            flag = false;
                            this._cieloError.Code = -2;
                            this._cieloError.Message = "O Payment.Type n\x00e3o foi informado";
                            return flag;
                        }
                        if (Convert.ToInt32(obj.Payment.Amount) == 0)
                        {
                            flag = false;
                            this._cieloError.Code = -3;
                            this._cieloError.Message = "O Amount n\x00e3o foi informado";
                            return flag;
                        }
                        if (Convert.ToInt32(obj.Payment.Installments) == 0)
                        {
                            flag = false;
                            this._cieloError.Code = -4;
                            this._cieloError.Message = "O Installments n\x00e3o foi informado";
                            return flag;
                        }
                        if (obj.Payment.RecurrentPayment == null)
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O dados da recorrencia n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.Interval))
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O Interval n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.CardNumber))
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O CardNumber n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.ExpirationDate))
                        {
                            flag = false;
                            this._cieloError.Code = -6;
                            this._cieloError.Message = "O ExpirationDate n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.SecurityCode))
                        {
                            flag = false;
                            this._cieloError.Code = -7;
                            this._cieloError.Message = "O SecurityCode n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.Brand))
                        {
                            flag = false;
                            this._cieloError.Code = -8;
                            this._cieloError.Message = "O Brand n\x00e3o foi informado";
                        }
                        return flag;
                    }
                    flag = false;
                    this._cieloError.Code = -1;
                    this._cieloError.Message = "O MerchantOrderId n\x00e3o foi informado";
                    return flag;

                case EnumTipoOperacao.RecorrenteComPeriodo:
                    if (!string.IsNullOrEmpty(obj.MerchantOrderId))
                    {
                        if (string.IsNullOrEmpty(obj.Payment.Type))
                        {
                            flag = false;
                            this._cieloError.Code = -2;
                            this._cieloError.Message = "O Payment.Type n\x00e3o foi informado";
                            return flag;
                        }
                        if (Convert.ToInt32(obj.Payment.Amount) == 0)
                        {
                            flag = false;
                            this._cieloError.Code = -3;
                            this._cieloError.Message = "O Amount n\x00e3o foi informado";
                            return flag;
                        }
                        if (Convert.ToInt32(obj.Payment.Installments) == 0)
                        {
                            flag = false;
                            this._cieloError.Code = -4;
                            this._cieloError.Message = "O Installments n\x00e3o foi informado";
                            return flag;
                        }
                        if (obj.Payment.RecurrentPayment == null)
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O dados da recorrencia n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.StartDate))
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O StartDate n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.EndDate))
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O EndDate n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.CardNumber))
                        {
                            flag = false;
                            this._cieloError.Code = -5;
                            this._cieloError.Message = "O CardNumber n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.ExpirationDate))
                        {
                            flag = false;
                            this._cieloError.Code = -6;
                            this._cieloError.Message = "O ExpirationDate n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.SecurityCode))
                        {
                            flag = false;
                            this._cieloError.Code = -7;
                            this._cieloError.Message = "O SecurityCode n\x00e3o foi informado";
                            return flag;
                        }
                        if (string.IsNullOrEmpty(obj.Payment.CreditCard.Brand))
                        {
                            flag = false;
                            this._cieloError.Code = -8;
                            this._cieloError.Message = "O Brand n\x00e3o foi informado";
                        }
                        return flag;
                    }
                    flag = false;
                    this._cieloError.Code = -1;
                    this._cieloError.Message = "O MerchantOrderId n\x00e3o foi informado";
                    return flag;
            }
            return flag;
        }

        private bool ValidaDadosIntegracaoRecorrente(CieloEnvioRecorrente obj)
        {
            bool flag = true;
            if (string.IsNullOrEmpty(obj.MerchantOrderId))
            {
                flag = false;
                this._cieloError.Code = -1;
                this._cieloError.Message = "O MerchantOrderId n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.Type))
            {
                flag = false;
                this._cieloError.Code = -2;
                this._cieloError.Message = "O Payment.Type n\x00e3o foi informado";
                return flag;
            }
            if (Convert.ToInt32(obj.Payment.Amount) == 0)
            {
                flag = false;
                this._cieloError.Code = -3;
                this._cieloError.Message = "O Amount n\x00e3o foi informado";
                return flag;
            }
            if (Convert.ToInt32(obj.Payment.Installments) == 0)
            {
                flag = false;
                this._cieloError.Code = -4;
                this._cieloError.Message = "O Installments n\x00e3o foi informado";
                return flag;
            }
            if (obj.Payment.RecurrentPayment == null)
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O dados da recorrencia n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.Interval))
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O Interval n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.CardNumber))
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O CardNumber n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.Holder))
            {
                flag = false;
                this._cieloError.Code = -6;
                this._cieloError.Message = "O Holder n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.ExpirationDate))
            {
                flag = false;
                this._cieloError.Code = -7;
                this._cieloError.Message = "O ExpirationDate n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.SecurityCode))
            {
                flag = false;
                this._cieloError.Code = -8;
                this._cieloError.Message = "O SecurityCode n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.Brand))
            {
                flag = false;
                this._cieloError.Code = -9;
                this._cieloError.Message = "O Brand n\x00e3o foi informado";
            }
            return flag;
        }

        private bool ValidaDadosIntegracaoRecorrenteEspecifico(CieloEnvioRecorrente obj)
        {
            bool flag = true;
            if (string.IsNullOrEmpty(obj.MerchantOrderId))
            {
                flag = false;
                this._cieloError.Code = -1;
                this._cieloError.Message = "O MerchantOrderId n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.Type))
            {
                flag = false;
                this._cieloError.Code = -2;
                this._cieloError.Message = "O Payment.Type n\x00e3o foi informado";
                return flag;
            }
            if (Convert.ToInt32(obj.Payment.Amount) == 0)
            {
                flag = false;
                this._cieloError.Code = -3;
                this._cieloError.Message = "O Amount n\x00e3o foi informado";
                return flag;
            }
            if (Convert.ToInt32(obj.Payment.Installments) == 0)
            {
                flag = false;
                this._cieloError.Code = -4;
                this._cieloError.Message = "O Installments n\x00e3o foi informado";
                return flag;
            }
            if (obj.Payment.RecurrentPayment == null)
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O dados da recorrencia n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.Interval))
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O Interval n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.StartDate))
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O StartDate n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.RecurrentPayment.EndDate))
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O EndDate n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.CardNumber))
            {
                flag = false;
                this._cieloError.Code = -5;
                this._cieloError.Message = "O CardNumber n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.Holder))
            {
                flag = false;
                this._cieloError.Code = -6;
                this._cieloError.Message = "O Holder n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.ExpirationDate))
            {
                flag = false;
                this._cieloError.Code = -7;
                this._cieloError.Message = "O ExpirationDate n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.SecurityCode))
            {
                flag = false;
                this._cieloError.Code = -8;
                this._cieloError.Message = "O SecurityCode n\x00e3o foi informado";
                return flag;
            }
            if (string.IsNullOrEmpty(obj.Payment.CreditCard.Brand))
            {
                flag = false;
                this._cieloError.Code = -9;
                this._cieloError.Message = "O Brand n\x00e3o foi informado";
            }
            return flag;
        }
    }
}

