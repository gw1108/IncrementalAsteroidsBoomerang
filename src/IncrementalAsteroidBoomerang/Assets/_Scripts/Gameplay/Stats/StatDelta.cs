public readonly struct StatDelta
{
    public readonly string         FieldKey;
    public readonly DeltaMode      Mode;
    public readonly DeltaValueType Type;
    public readonly int            IntValue;
    public readonly float          FloatValue;

    public StatDelta(string fieldKey, DeltaMode mode, int value)
    {
        FieldKey   = fieldKey;
        Mode       = mode;
        Type       = DeltaValueType.Int;
        IntValue   = value;
        FloatValue = 0f;
    }

    public StatDelta(string fieldKey, DeltaMode mode, float value)
    {
        FieldKey   = fieldKey;
        Mode       = mode;
        Type       = DeltaValueType.Float;
        FloatValue = value;
        IntValue   = 0;
    }
}
