Public Interface IQTreeTraverser
    Delegate Sub ProgressUpdate(ByVal nodesTraversed As Int32)
    ReadOnly Property NodesTraversed() As Int32
    Sub Apply(ByRef node As QTreeNodeLeaf)
    Sub Apply(ByRef node As QTreeNodeBranch)

End Interface

Public Interface ITraversable
    Sub traverse(ByRef traverser As IQTreeTraverser)
End Interface

