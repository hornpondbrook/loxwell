namespace loxwell;

public class Return : Exception {
  internal readonly object Value;

  public Return(object value) {
    Value = value;
  }

}