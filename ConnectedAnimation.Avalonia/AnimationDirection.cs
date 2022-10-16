namespace ConnectedAnimation.Avalonia;

[Flags]
internal enum AnimationDirection
{
    None,

    // 上がる ⤴
    Upper = 1,

    // 下がる ⤵
    Lower = 1 << 1,

    Left = 1 << 2,
    Right = 1 << 3,

    LeftUpper = Left | Upper,
    RightUpper = Right | Upper,
    RightLower = Right | Lower,
    LeftLower = Left | Lower,
}
