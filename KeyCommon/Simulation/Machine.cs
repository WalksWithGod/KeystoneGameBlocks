using System;


namespace KeyCommon.Simulation
{
    ///// <summary>
    ///// A game object that produces and consumes 
    ///// </summary>
    //public class Machine : DomainObject
    //{
    //    // a specific type of DomainObject has these properties
    //    // here i can keep my hierarchy flat though. 

    //    // what if i need to grab these consumption/production values
    //    // from a xml or db though?  I dont want to have DomainObjects as Nodes
    //    // but i do maybe still want ability to load data so that changing the central
    //    // data will automatically change all Machines
    //    //
    //    // I think the easiest solution is to simply allow this to implement our persist
    //    // mechanism and then to essentially have it's own seperate set of tables in our database
    //    // ???  
    //    public Production[] Consumption;  // what this component consumes during operation
    //    public Production[] Production;   // what this component produces during operation
    //    public UInt32[] Store;        // onboard store of products

    //    // todo; im torn, why shouldnt this be a custom property?
    //    public float Damage;


    //    public Machine(string id) : base(id) { }

    //    public void Receive(UInt32 product, float quantity)
    //    {
    //    }

    //    public void Send(UInt32 product, float quantity)
    //    {

    //    }
    //}
}
