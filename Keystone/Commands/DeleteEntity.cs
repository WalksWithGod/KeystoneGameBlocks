namespace Keystone.Commands
{
    //public class DeleteEntity : ICommand
    //{
    //    IEntity _entity;
    //    public string Name;
    //    PostExecuteWorkItemCallback _completionCB;

    //    public DeleteEntity(IEntity entity, PostExecuteWorkItemCallback completionCB)
    //    {

    //        _entity = entity;
    //        Name = this.GetType().Name;
    //        _completionCB = completionCB;
    //    }

    //    public object Execute()
    //    {
    //        return Worker();
    //    }

    //    // todo:
    //    public void UnExecute()
    //    {
    //        //    // call to remove should handle derefercing the node and such
    //        //    Core._CoreClient.SceneManager.CurrentScene.Remove(Model);

    //        //    Core._CoreClient.Simulation.RemoveEntity(Entity);
    //    }

    //    protected object Worker()
    //    {

    //        try
    //        {
    //            Core._CoreClient.Simulation.RemoveEntity(_entity);   

    //        }
    //        // todo: we have to unroll anything we've done here if the command fails we want it to fail completely, not partially
    //        // and having all the stages is also important for Undo.
    //        // but that is difficult if we do NOT divide RemoveEntity into multiple steps so that we know exactly which child entities were removed.
    //        // we could recurse the entity here and build the sublist of commands and process those here and then return the entire
    //        // command after all are finished.
    //        // todo: also we need to guard against somehow a second thread removing a part of the entity (somehow) before this has completed.
    //        catch (Exception ex)
    //        {
    //            // todo: need to trap the exception, unroll any changes and then throw a new exception for the IWorkItemResult.Exception()
    //            Trace.WriteLine(string.Format("Importer.Instantiate() -- Error creating node '{0}'.", classname));
    //        }

    //        return null;
    //    }
    //}
}