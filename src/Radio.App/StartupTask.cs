using Windows.ApplicationModel.Background;
using DryIoc;
using Radio.Lib;
using System.Diagnostics;
using System;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Radio.App {
  public sealed class StartupTask : IBackgroundTask {

    public async void Run(IBackgroundTaskInstance taskInstance) {
      var deferral = taskInstance.GetDeferral();
      try {
        using (var container = Bootstrapper.CreateContainer()) {
          using (var radio = container.Resolve<IRadioService>()) {
            await radio.RunServiceAsync().ConfigureAwait(false);
          }
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex);
      } finally {
        deferral.Complete();
      }
    }
  }
}
