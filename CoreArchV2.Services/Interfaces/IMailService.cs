using CoreArchV2.Dto.ECommonDto;

namespace CoreArchV2.Services.Interfaces
{
    public interface IMailService
    {
        bool SendMail(string mailTo, string subject, string body, string mailCcs = "", string mailBccs = "");
        bool SendMailWithAttach(EMailDto model);
        //EResultDto SendMail(List<User> mailTo, string subject, string body);
    }
}
