﻿
namespace TestApp.AnalogDigitalConverter {
  internal struct PortValue<TValue> {
    public byte Port { get; }
    public TValue Value { get; }

    public PortValue(byte port, TValue value) {
      Port = port;
      Value = value;
    }

  }
}
