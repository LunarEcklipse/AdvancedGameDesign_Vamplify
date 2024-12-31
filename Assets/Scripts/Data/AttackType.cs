public enum AttackType
{
    Point, // Attacks at one square. Can attack up to range away.
    Line, // Attacks in a straight line. Can attack up to range away. Radius determines width of box cast.
    Circle, // Attacks in a circle. Can attack up to range away. Radius determines width.
    AroundSelf, // Attacks around self. Range is ignored. Radius determines width.
    Cone, // Cone attack. Can attack up to range away. Radius determines angle.
    Undefined
}