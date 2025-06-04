Public Class NullCell : Inherits Cell
    Private Const NULL_COORD As Single = -9999999
    Public Sub New()
        MyBase.New(New MTV3D65.TV_3DVECTOR(NULL_COORD, NULL_COORD, NULL_COORD), 0, 0, 0)
    End Sub

    Public Shared Function IsNullCell(ByVal key As MTV3D65.TV_3DVECTOR) As Boolean
        Return key.x = NULL_COORD AndAlso key.y = NULL_COORD AndAlso key.z = NULL_COORD
    End Function
End Class
