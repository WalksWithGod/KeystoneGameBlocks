Public Class SubNet
    Public Bounds As BoundingBox
    Public Areas() As Area

End Class

Public Class NavNetRuntimeManager

    Private _currentSubNet As SubNet
    Private _graphs() As SubNet


    Public Sub SetCurrentNet(ByVal actorPosition As MTV3D65.TV_3DVECTOR)
        Dim sec As Area
        For Each g As SubNet In _graphs
            sec = AreaPicker.FindArea(_currentSubNet.Areas, actorPosition)
            If sec IsNot Nothing Then
                _currentSubNet = g
                Exit For
            End If
        Next
        Debug.Assert(_currentSubNet IsNot Nothing)
    End Sub

    Public ReadOnly Property CurrentSubNet() As SubNet
        Get
            Return _currentSubNet
        End Get
    End Property
End Class
