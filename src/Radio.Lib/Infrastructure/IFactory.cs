using System.Threading;
using System.Threading.Tasks;

namespace Radio.Lib.Infrastructure {
  public interface IFactory<TService> {
    Task<TService> CreateAsync(CancellationToken cancellation_token = default(CancellationToken));
  }
}
