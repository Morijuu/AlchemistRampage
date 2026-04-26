public enum BulletType
{
    None,
    Regular,
    Heavy,
    Bouncy,
    Area,    // Regular + Heavy
    Frag,    // Heavy + Bouncy
    Target,  // Regular + Bouncy
    Chain,   // Area + Bouncy
    Piercing // Target + Heavy
}
