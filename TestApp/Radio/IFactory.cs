namespace TestApp {
  public interface IFactory<TService> {
    TService Create();
  }
}
