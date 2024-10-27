namespace Darkness
{
    public delegate void ValueChangedEvent<in T>(T oldValue, T newValue);
}