using System.Threading;
using System.Threading.Tasks;
using RotinaBot.Documents;

namespace RotinaBot
{
    public interface ISMSSender
    {
        Task SendSMSAsync(Routine routine, CancellationToken cancellationToken);
    }
}