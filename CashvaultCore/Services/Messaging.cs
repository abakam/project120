using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using CashvaultCore.Utilities;
using MailChimp;
using MailChimp.Types;
using static MailChimp.Types.Mandrill.Messages;
using CashvaultCore.Model;
using System.Runtime.Serialization.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using SendGrid.CSharp.HTTP.Client;
using System.Text;

namespace CashvaultCore.Services
{
    public class Messaging
    {
        static string logPath = ConfigurationManager.AppSettings["LoggingPath"];
        static string ErrorLogPath = ConfigurationManager.AppSettings["ErrorLoggingPath"];
        static string SendGridAPIKey = ConfigurationManager.AppSettings["SendGridAPIKey"];

        public bool SendSms(string sender, string message, string phoneNumber, out string statusName)
        {
            statusName = "UNKNOWN";
            var result = false;
            phoneNumber = phoneNumber.StartsWith("234") ? phoneNumber : "234" + phoneNumber.TrimStart('0');
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["SMSURL"]);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, ConfigurationManager.AppSettings["SMSAuthorization"]);

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    var json = "{ \"from\": \"" + sender + "\", " +
                                  " \"to\": \"" + phoneNumber + "\", " +
                                  " \"text\": \"" + message + "\" " +
                                  "}";

                    Logger.logToFile("C:\\CashVault\\Logs\\DebugTrace\\SMS\\", "Send-" + phoneNumber + "-" + Guid.NewGuid() + ".txt", json);

                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                var responseText = string.Empty;
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var ser = new DataContractJsonSerializer(typeof(SMSResponse));
                    var stream = httpResponse.GetResponseStream();
                    if (stream != null)
                    {
                        var infobipSmsResponse = (SMSResponse)ser.ReadObject(stream);

                        statusName = infobipSmsResponse.messages[0].status.name;

                        if (infobipSmsResponse.messages[0].status.name == "MESSAGE_ACCEPTED"
                            || infobipSmsResponse.messages[0].status.name == "PENDING_WAITING_DELIVERY"
                            || infobipSmsResponse.messages[0].status.name == "PENDING_ENROUTE"
                            || infobipSmsResponse.messages[0].status.name == "PENDING_ACCEPTED")
                        {
                            result = true;
                        }

                        responseText = XMLHelper.ConvertObjectToXmlString(infobipSmsResponse);
                    }
                }

                Logger.logToFile("C:\\CashVault\\Logs\\DebugTrace\\SMS\\", "Response-" + phoneNumber + "-" + Guid.NewGuid() + ".xml", responseText);

            }
            catch (Exception ex)
            {
                Logger.logToFile(ex.ToString(), ErrorLogPath, true, "SMS Messaging");
            }
            return result;
        }

        public bool SendSmsBulksmsNigeria(string sender, string message, string phoneNumber, out string statusName)
        {
            statusName = "UNKNOWN";
            var result = false;
            phoneNumber = phoneNumber.StartsWith("234") ? phoneNumber : "234" + phoneNumber.TrimStart('0');           

            try
            {
                string messageURLText = ConfigurationManager.AppSettings["BulkSMSNigeriaURL"] + "&from=" + sender;
                messageURLText += "&to=" + phoneNumber + "&DND="+ ConfigurationManager.AppSettings["BulkSMSNigeriaDND"] + "&body=" + message;
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(messageURLText);
                httpWebRequest.Method = "POST";

                var json = "{ \"from\": \"" + sender + "\", " +
                              " \"to\": \"" + phoneNumber + "\", " +
                              " \"text\": \"" + message + "\" " +
                              "}";

                Logger.logToFile("C:\\CashVault\\Logs\\DebugTrace\\SMS\\", "Send-" + phoneNumber + "-" + Guid.NewGuid() + ".txt", json);

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                var responseText = string.Empty;
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var ser = new DataContractJsonSerializer(typeof(BulksmsNigeriaResponse));
                    var stream = httpResponse.GetResponseStream();
                    if (stream != null)
                    {
                        var SmsResponse = (BulksmsNigeriaResponse)ser.ReadObject(stream);

                        statusName = SmsResponse.data.status;

                        if (statusName == "success")
                        {
                            result = true;
                        }
                        responseText = XMLHelper.ConvertObjectToXmlString(SmsResponse);
                    }
                }

                Logger.logToFile("C:\\CashVault\\Logs\\DebugTrace\\SMS\\", "Response-" + phoneNumber + "-" + Guid.NewGuid() + ".xml", responseText);

            }
            catch (Exception ex)
            {
                Logger.logToFile(ex.ToString(), ErrorLogPath, true, "SMS Messaging");
            }
            return result;
        }
        
        public bool MandrilEmail(string Subject, string Message, string Email, string From)
        {
            bool result = false;
            MVList<Mandrill.Messages.SendResult> MailResult = null;
            try
            {
                string apiKey = ConfigurationManager.AppSettings["EmailPassword"];
                var api = new MandrillApi(apiKey);
                var recipients = new List<Mandrill.Messages.Recipient>();

                var name = string.Format("{0}", Email);
                recipients.Add(new Mandrill.Messages.Recipient(Email, name));
                var message = new Mandrill.Messages.Message()
                {
                    To = recipients.ToArray(),
                    FromEmail = From,
                    Subject = Subject,
                    Html = Message,
                    FromName = ConfigurationManager.AppSettings["EmailFromName"]
                };
                MailResult = api.Send(message);
                Status resultStatus = MailResult[0].Status;

                if (resultStatus == Status.Sent || resultStatus == Status.Queued)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Logger.logToFile(ex.ToString(), ErrorLogPath, true, "Mandrill Emai Messaging", "txt", true);
            }
            return result;
        }

        public bool SendgridEmailV2(string Subject, string Message, string Email, string From, string fromName)
        {
            var result = SendgridEmailV2Async(Subject, Message, Email, From, fromName).GetAwaiter().GetResult();
            Response response = null;
            if (result != null)
            {
                response = (Response)result;
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted)
                    return true;
                else
                {

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Email send to " + Email + " failed.");
                    sb.AppendLine(response.StatusCode.ToString());
                    sb.AppendLine(response.Body.ReadAsStringAsync().Result);
                    sb.AppendLine(response.Headers.ToString());

                    Logger.logToFile(sb.ToString(), ErrorLogPath, true, "Sendgrid Email Messaging", "txt", true);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<dynamic> SendgridEmailV2Async(string Subject, string Message, string Email, string From, string fromName)
        {

            dynamic sg = new SendGridAPIClient(SendGridAPIKey);

            Email from = new Email(From, fromName);
            string subject = Subject;
            Email to = new Email(Email);

            Content content = new Content("text/html", Message);
            Mail mail = new Mail(from, subject, to, content);

            var response = await sg.client.mail.send.post(requestBody: mail.Get());

            #if DEBUG
            Console.WriteLine(response.StatusCode);
            Console.WriteLine(response.Body.ReadAsStringAsync().Result);
            Console.WriteLine(response.Headers.ToString());
            Console.WriteLine(mail.Get());
            #endif

            return response;
        }

    }
}
