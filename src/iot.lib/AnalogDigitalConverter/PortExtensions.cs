using System;
using System.Linq;
using System.Reactive.Linq;

namespace iot.lib.AnalogDigitalConverter {
  public static class PortExtensions {

    public static IObservable<PortValue<TOutput>> MapAndDistinct<TInput, TOutput>(this IObservable<PortValue<TInput>> stream, Func<PortValue<TInput>, PortValue<TOutput>> map_func)
      => stream
      .GroupBy(value => value.Port)
      .SelectMany(port => port.Select(map_func).DistinctUntilChanged(value => value));
  }

}
