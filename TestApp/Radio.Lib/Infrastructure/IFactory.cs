namespace Radio.Lib.Infrastructure {
  public interface IFactory<TService> {
    TService Create();
  }
}
