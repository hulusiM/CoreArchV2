using CoreArchV2.Dto.ECommonDto;
using CoreArchV2.Services.Interfaces;
using CoreArchV2.Utilies;
using EASendMail;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace CoreArchV2.Services.Services
{
    public class MailService : IMailService
    {
        private readonly MailSetting _mailSetting;
        public MailService(IOptions<MailSetting> mailSetting)
        {
            _mailSetting = mailSetting.Value;
        }
        public bool SendMail(string mailTo, string subject, string body, string mailCcs = "", string mailBccs = "")
        {
            if (_mailSetting.MethodNo == "1")
                return SendMail1(mailTo, subject, body, mailCcs, mailBccs);
            else if (_mailSetting.MethodNo == "2")
                return SendMail2(mailTo, subject, body, mailCcs, mailBccs);

            return false;
        }

        public bool SendMail1(string mailTo, string subject, string body, string mailCcs = "", string mailBccs = "")
        {
            try
            {
                var mailToList = mailTo.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                var mailCcList = new List<string>();
                if (!string.IsNullOrEmpty(mailCcs))
                    mailCcList = mailCcs.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                var mailBccList = new List<string>();
                if (!string.IsNullOrEmpty(mailBccs))
                    mailBccList = mailBccs.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                if (mailToList.Any())
                {
                    System.Net.Mail.SmtpClient server = new System.Net.Mail.SmtpClient(_mailSetting.Host);
                    server.Port = _mailSetting.Port;
                    server.EnableSsl = false;
                    server.Credentials = new System.Net.NetworkCredential(_mailSetting.FromEmail, _mailSetting.Pass);
                    server.Timeout = 5000;
                    server.UseDefaultCredentials = false;

                    MailMessage mail = new MailMessage();
                    mail.From = new System.Net.Mail.MailAddress(_mailSetting.FromEmail, _mailSetting.DisplayName);

                    foreach (var item in mailToList)
                        mail.To.Add(item);

                    foreach (var item in mailCcList)
                        mail.CC.Add(item);

                    foreach (var item in mailBccList)
                        mail.Bcc.Add(item);

                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    server.Send(mail);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public bool SendMail2(string mailTo, string subject, string body, string mailCcs = "", string mailBccs = "")
        {
            try
            {
                var mailToList = mailTo.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                var mailCcList = new List<string>();
                if (!string.IsNullOrEmpty(mailCcs))
                    mailCcList = mailCcs.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                var mailBccList = new List<string>();
                if (!string.IsNullOrEmpty(mailBccs))
                    mailBccList = mailBccs.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                if (mailToList.Any())
                {
                    EASendMail.SmtpMail oMail = new EASendMail.SmtpMail("TryIt");

                    oMail.From = new EASendMail.MailAddress(_mailSetting.DisplayName, _mailSetting.FromEmail);

                    foreach (var item in mailToList)
                        oMail.To.Add(item);

                    foreach (var item in mailCcList)
                        oMail.Cc.Add(item);

                    foreach (var item in mailBccList)
                        oMail.Bcc.Add(item);

                    oMail.Subject = subject;
                    //oMail.TextBody = body;
                    oMail.HtmlBody = body;

                    EASendMail.SmtpServer oServer = new EASendMail.SmtpServer(_mailSetting.Host);
                    oServer.User = _mailSetting.FromEmail;
                    oServer.Password = _mailSetting.Pass;

                    oServer.Port = 465;
                    oServer.ConnectType = SmtpConnectType.ConnectDirectSSL;

                    EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
                    oSmtp.SendMail(oServer, oMail);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendMailWithAttach(EMailDto model)
        {
            if (_mailSetting.MethodNo == "1")
                return SendMailWithAttach1(model);
            else if (_mailSetting.MethodNo == "2")
                return SendMailWithAttach2(model);

            return false;
        }

        public bool SendMailWithAttach1(EMailDto model)
        {
            try
            {
                System.Net.Mail.SmtpClient server = new System.Net.Mail.SmtpClient(_mailSetting.Host);
                server.Port = 25;
                server.EnableSsl = false;
                server.Credentials = new System.Net.NetworkCredential(_mailSetting.FromEmail, _mailSetting.Pass);
                server.Timeout = 5000;
                server.UseDefaultCredentials = false;
                MailMessage msg = new MailMessage();
                msg.From = new System.Net.Mail.MailAddress(_mailSetting.FromEmail, _mailSetting.DisplayName);
                msg.Subject = model.Subject;
                msg.Body = model.Body;
                msg.IsBodyHtml = true;

                var mailTo = new List<string>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(model.To))
                    {
                        if (model.To.Split(";").Length > 1)
                        {
                            mailTo = model.To.Split(";").ToList();
                            mailTo = mailTo.Where(w => w != "").Distinct().ToList();

                            foreach (var item in mailTo)
                                msg.To.Add(item);
                        }
                        else
                        {
                            mailTo.Add(model.To);
                            msg.To.Add(model.To);
                        }
                    }
                }
                catch (Exception e) { }

                var mailCc = new List<string>();
                try
                {
                    if (!string.IsNullOrWhiteSpace(model.Cc))
                    {
                        if (model.Cc.Split(";").Length > 1)
                        {
                            mailCc = model.Cc.Split(";").ToList();
                            mailCc = mailCc.Where(w => w != "").Distinct().ToList();

                            mailCc = mailCc.Except(mailTo).ToList(); //to ve cc içinde aynı mail olunca hata veriyor. 

                            if (mailCc.Count > 0)
                            {
                                foreach (var item in mailCc)
                                    msg.CC.Add(item);
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(model.To) || (model.To != null && !model.To.Contains(model.Cc))) //to içinde cc mail adresi varsa eklemesin
                            {
                                mailCc.Add(model.Cc);
                                msg.CC.Add(model.Cc);
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    if (!string.IsNullOrWhiteSpace(model.Bcc))
                    {
                        if (model.Bcc.Split(";").Length > 1)
                        {
                            var mailBcc = model.Bcc.Split(";").ToList();
                            mailBcc = mailBcc.Where(w => w != "").Distinct().ToList();

                            mailBcc = mailBcc.Except(mailTo).ToList();

                            mailBcc = mailBcc.Except(mailCc).ToList();

                            if (mailBcc.Count > 0)
                            {
                                foreach (var item in mailBcc)
                                    msg.Bcc.Add(item);
                            }
                        }
                        else
                        {
                            bool isAddBcc = true;

                            if (model.To != null && model.To.Contains(model.Bcc))
                                isAddBcc = false;
                            else if (model.Cc != null && model.Cc.Contains(model.Bcc))
                                isAddBcc = false;

                            if (isAddBcc)
                                msg.Bcc.Add(model.Bcc);
                        }
                    }
                }
                catch
                {
                }

                foreach (var file in model.Attachments)
                {
                    System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
                    if (file.Extention.ToLower().Contains("jpeg"))
                        contentType.MediaType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                    else if (file.Extention.ToLower().Contains("png"))
                        contentType.MediaType = "image/png";
                    else if (file.Extention.ToLower().Contains("jpg"))
                        contentType.MediaType = "image/jpg";
                    else if (file.Extention.ToLower().Contains("pdf"))
                        contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Pdf;

                    contentType.Name = file.Name;

                    msg.Attachments.Add(new System.Net.Mail.Attachment(model.RootUrl + file.Name, contentType));
                }

                server.Send(msg);
            }
            catch (Exception e) { return false; }
            return true;
        }

        public bool SendMailWithAttach2(EMailDto model)
        {
            try
            {
                var mailToList = model.To.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                var mailCcList = new List<string>();
                if (!string.IsNullOrEmpty(model.Cc))
                    mailCcList = model.Cc.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                var mailBccList = new List<string>();
                if (!string.IsNullOrEmpty(model.Bcc))
                    mailBccList = model.Bcc.Split(";").Where(w => w.Length > 0).Distinct().ToList();

                if (mailToList.Any())
                {
                    SmtpMail oMail = new SmtpMail("TryIt");

                    oMail.From = new EASendMail.MailAddress(_mailSetting.DisplayName, _mailSetting.FromEmail);

                    foreach (var item in mailToList)
                        oMail.To.Add(item);

                    foreach (var item in mailCcList)
                        oMail.Cc.Add(item);

                    foreach (var item in mailBccList)
                        oMail.Bcc.Add(item);

                    oMail.Subject = model.Subject;
                    //oMail.TextBody = body;
                    oMail.HtmlBody = model.Body;

                    foreach (var file in model.Attachments)
                    {
                        System.Net.Mime.ContentType contentType = new System.Net.Mime.ContentType();
                        if (file.Extention.ToLower().Contains("jpeg"))
                            contentType.MediaType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        else if (file.Extention.ToLower().Contains("png"))
                            contentType.MediaType = "image/png";
                        else if (file.Extention.ToLower().Contains("jpg"))
                            contentType.MediaType = "image/jpg";
                        else if (file.Extention.ToLower().Contains("pdf"))
                            contentType.MediaType = System.Net.Mime.MediaTypeNames.Application.Pdf;

                        contentType.Name = file.Name;

                        oMail.AddAttachment(model.RootUrl + file.Name);
                    }


                    SmtpServer oServer = new SmtpServer(_mailSetting.Host);
                    oServer.User = _mailSetting.FromEmail;
                    oServer.Password = _mailSetting.Pass;

                    oServer.Port = 465;
                    oServer.ConnectType = SmtpConnectType.ConnectDirectSSL;

                    EASendMail.SmtpClient oSmtp = new EASendMail.SmtpClient();
                    oSmtp.SendMail(oServer, oMail);

                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
