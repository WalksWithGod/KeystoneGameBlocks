namespace Keystone.Enums
{
    public enum MOUSE_BUTTONS : int
    {
        XAXIS = 0, // TODO: why is this behaving as left mouse button?!
        YAXIS = 4,
        BUTTON4 = 3,
        BUTTON5 = 5,
        BUTTON6 = 6,
        BUTTON7 = 7,
        WHEEL = 8,
        LEFT = 12,
        MIDDLE = 13,
        RIGHT = 14
    }
    //public enum MouseState
    //{
    //    XAxisAB = -2113928704,
    //    YAxisAB = -2113928700,
    //    XAxis = -2113928448,
    //    YAxis = -2113928444,
    //    Wheel = -2113928440,
    //    Button0 = -2113928180,
    //    Button1 = -2113928179,
    //    Button2 = -2113928178,
    //    Button3 = -2113928177,
    //    Button4 = -2113928176,
    //    Button5 = -2113928175,
    //    Button6 = -2113928174,
    //    Button7 = -2113928173,
    //}
    // from http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=326066&SiteID=1
    //i.e. for Button0, Button1 the 32-bit hex values are 0x8200040C, 0x8200040D where by clicking mouse buttons they appear as 0x0000000C, 0x0000000D,... so maybe one could follow this path (would need to test more to be sure..)

    //Maybe the Mouse type lacks an explicit type conversion operator, that would care for correct retyping...? .. possible for enums at all? (Eh, not sure what I'm telling right now, forget it. ;)
    //Do you think the cause of the problem is a bug in MDX? Such workarounds seem a bit suspicious to me...  

}