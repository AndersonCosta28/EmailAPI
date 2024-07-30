using Microsoft.AspNetCore.Mvc;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
namespace EmailAPI.Controllers;

[Route("[controller]")]
public class EmailController(ILogger<EmailController> logger, IConfiguration configuration) : ControllerBase
{
    [HttpGet("Outlook")]
    public IActionResult LerEmailOutlook([FromQuery] string assunto, [FromQuery] string emissor)
    {
        var usuario = configuration["Outlook:Usuario"];
        var senha = configuration["Outlook:Senha"];
        var servidorOutlook = "outlook.office365.com";
        var portaOutlook = 993;
        var usaSSLOutlook = true;
        EntrarNoEmail(servidorOutlook, portaOutlook, usaSSLOutlook, usuario, senha, emissor, emissor);
        return Ok();
    }

    [HttpGet("Gmail")]
    public IActionResult LerEmailGmail([FromQuery] string assunto, [FromQuery] string emissor)
    {
        var usuario = configuration["Gmail:Usuario"];
        var senha = configuration["Gmail:Senha"];
        var servidorGoogle = "imap.gmail.com";
        var portaGoogle = 993;
        var usaSSLGoogle = true;
        EntrarNoEmail(servidorGoogle, portaGoogle, usaSSLGoogle, usuario, senha, emissor, assunto);
        return Ok();
    }

    private void EntrarNoEmail(string servidor, int porta, bool usarSSL, string usuario, string senha, string emissor, string assunto)
    {
        using var cliente = new ImapClient();
        cliente.Connect(servidor, porta, usarSSL);
        cliente.Authenticate(usuario, senha);
        cliente.Inbox.Open(FolderAccess.ReadOnly);

        SearchQuery query;

        if (!string.IsNullOrEmpty(emissor) && !string.IsNullOrEmpty(assunto))
        {
            // Filtrar por emissor e assunto
            query = SearchQuery.And(SearchQuery.FromContains(emissor), SearchQuery.SubjectContains(assunto));
        }
        else if (!string.IsNullOrEmpty(emissor))
        {
            // Filtrar apenas por emissor
            query = SearchQuery.FromContains(emissor);
        }
        else if (!string.IsNullOrEmpty(assunto))
        {
            // Filtrar apenas por assunto
            query = SearchQuery.SubjectContains(assunto);
        }
        else
        {
            // Sem filtros
            query = SearchQuery.All;
        }

        var uudis = cliente.Inbox.Search(query);
        foreach (var uniqueId in uudis)
        {
            var mensagemDoEmail = cliente.Inbox.GetMessage(uniqueId);
            logger.LogInformation(mensagemDoEmail.TextBody);
        }
        cliente.Disconnect(true);
    }
}

