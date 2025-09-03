public class ColorUiModel
{
    public int ColorId { get; set; }
    public string Name { get; set; } // 表示名（例："赤"）
    public Color ColorValue { get; set; } // 表示用Color
    public override bool Equals(object obj)
    {
        return obj is ColorUiModel other && this.ColorId == other.ColorId;
    }

    public override int GetHashCode()
    {
        return ColorId;
    }
}
