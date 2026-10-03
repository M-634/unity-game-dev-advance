using System;

namespace DemoGame
{
    // 解像度の値。Unity の Resolution はリフレッシュレートまで持つので、幅と高さだけの型にする。
    public readonly struct ScreenResolution : IEquatable<ScreenResolution>
    {
        public static readonly ScreenResolution Native = new(0, 0);

        public readonly int Width;
        public readonly int Height;

        public ScreenResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public bool IsNative => Width <= 0 || Height <= 0;

        public bool Equals(ScreenResolution other) => Width == other.Width && Height == other.Height;

        public override bool Equals(object obj) => obj is ScreenResolution other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Width, Height);

        public override string ToString() => IsNative ? "Native" : $"{Width} x {Height}";
    }
}
