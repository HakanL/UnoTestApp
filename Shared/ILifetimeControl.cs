using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXCore.DMXCore100;

public interface ILifetimeControl
{
    void Shutdown();

    IObservable<bool> Running { get; }

    void RegisterShutdownEvent(Action shutdownEvent);

    void SetSystemShuttingDown();

    void SetSystemShutdownEvent();

    void Exit();
}
