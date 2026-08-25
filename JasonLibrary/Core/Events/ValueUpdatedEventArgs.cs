namespace JasonLibrary.Core.Events
{
    public delegate void ValueUpdatedEventHandler(object sender, ValueUpdatedEventArgs e);

    public class ValueUpdatedEventArgs : System.EventArgs
    {
        public ValueUpdatedEventArgs(string newValue)
        {
            NewValue = newValue;
        }

        public string NewValue { get; }
    }
}
