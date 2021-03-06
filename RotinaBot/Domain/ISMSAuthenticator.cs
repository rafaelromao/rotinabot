using System.Threading;
using System.Threading.Tasks;
using RotinaBot.Documents;

namespace RotinaBot.Domain
{
    public interface ISMSAuthenticator
    {
        string GenerateAuthenticationCode();
        Task SendSMSAsync(Routine routine, CancellationToken cancellationToken);
    }
}