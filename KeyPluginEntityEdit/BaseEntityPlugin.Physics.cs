using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;

namespace KeyPlugins
{
    // physics panel
    partial class BaseEntityPlugin
    {
        // we need buttons to add RigidBody and Colliders
        // buttons to remove them
        // properties we can modify based on which node we are trying to edit.
        // not sure how we would handle compound colliders.  I think we decided
        // at one point to just have multiple child entities with one collider of their own.

        // should i create new "cards" like i use for appearance Material and Textures?
        // or do we just use a Property Panel like we use for Script properties.
        // I think I prefer the cards as they represent a group and we can quickly see which
        // collider and RigidBody is added to the selected Entity.
        // and in the future, we can maybe re-arrange the cards to go from top to bottom
        // rather than left to right (i.e. if we move the panel to dock along right side of editor)

        private void buttonAddRigidBody_Click(object sender, EventArgs e)
        {
            mHost.Node_Create("RigidBody", mTargetNodeID);
            //ClearPhysicsCardsPanel(superTabControlPanel9);
            CreatePhysicseCards(mTargetNodeID);
        }

        private void buttonAddBoxCollider_Click(object sender, EventArgs e)
        {
            mHost.Node_Create("BoxCollider", mTargetNodeID);
            ClearPhysicsCardsPanel(superTabControlPanel9);
            CreatePhysicseCards(mTargetNodeID);
        }

        private void buttonAddSphereCollider_Click(object sender, EventArgs e)
        {
            mHost.Node_Create("SphereCollider", mTargetNodeID);
            ClearPhysicsCardsPanel(superTabControlPanel9);
            CreatePhysicseCards(mTargetNodeID);
        }

        private void buttonAddCapsuleCollider_Click(object sender, EventArgs e)
        {
            mHost.Node_Create("CapsuleCollider", mTargetNodeID);
            ClearPhysicsCardsPanel(superTabControlPanel9);
            CreatePhysicseCards(mTargetNodeID);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityID">Entity Node ID</param>
        private void CreatePhysicseCards(string entityID)
        {
            ClearPhysicsCardsPanel(superTabControlPanel9);

            string[] childIDs;
            string[] childTypes;

            mHost.Node_GetChildrenInfo(entityID, null, out childIDs, out childTypes);

            if (childIDs != null && childIDs.Length > 0)
            {
                for (int i = 0; i < childIDs.Length; i++)
                {
                    string nodeType = childTypes[i];
                    string nodeID = childIDs[i];

                    switch (nodeType)
                    {
                        case "RigidBody":
                            {
                                RigidBodyEditCard physicsEditCard = new RigidBodyEditCard(nodeID, entityID);
                                physicsEditCard.Text = nodeType;

                                // create the properties for the rigid body and add them to the property grid
                                Settings.PropertySpec[] props = new Settings.PropertySpec[4];
                                // ConverterTypeName used to convert user input into correct property value
                                string category = "misc";

                                object value = mHost.Node_GetProperty(nodeID, "mass");
                                props[0] = new Settings.PropertySpec("mass", typeof(double), category, (object)value, typeof(double));

                                value = mHost.Node_GetProperty(nodeID, "static");
                                props[1] = new Settings.PropertySpec("static", typeof(bool), category, (object)value, typeof(bool));

                                value = mHost.Node_GetProperty(nodeID, "drag");
                                props[2] = new Settings.PropertySpec("drag", typeof(double), category, (object)value, typeof(double));

                                value = mHost.Node_GetProperty(nodeID, "kinematic");
                                props[3] = new Settings.PropertySpec("kinematic", typeof(bool), category, (object)value, typeof(bool));

                                physicsEditCard.ControlClosed += OnPhysicsNodeRemoved;
                                physicsEditCard.OnPropertyValueChanged += OnRigidBodyPropertyValueChanged;

                                physicsEditCard.SetGridProperties(props);

                                superTabControlPanel9.Controls.Add(physicsEditCard);
                            }
                            break;

                        case "BoxCollider":
                            {
                                RigidBodyEditCard physicsEditCard = new RigidBodyEditCard(nodeID, entityID);
                                physicsEditCard.Text = nodeType;

                                superTabControlPanel9.Controls.Add(physicsEditCard);

                                // TODO: how do i assign multiple box colliders to an entity such as one for elevator floor and
                                // another for elevator entrance/exit trigger zone?  I think i decided i would use
                                // multiple entities... trigger zone being invisible entity
                                // TODO: we need to filter some of these properties like "id", "name", etc.
                                // that's why its probably better to just gather the properties we know we want
                                // one at a time and build the properspec array and pass it to the card.
                                //Settings.PropertySpec[] properties = mHost.Node_GetProperties (entityID);
                                Settings.PropertySpec[] props = new Settings.PropertySpec[3];
                                string category = "misc";

                                object value = mHost.Node_GetProperty(nodeID, "trigger");
                                props[0] = new Settings.PropertySpec("trigger", typeof(bool),category, (object)value, typeof(bool));

                                value = mHost.Node_GetProperty(nodeID, "center");
                                props[1] = new Settings.PropertySpec("center", typeof(Vector3d), category, (object)value, typeof(Vector3d) );

                                value = mHost.Node_GetProperty(nodeID, "size");
                                props[2] = new Settings.PropertySpec("size", typeof(Vector3d), category, (object)value, typeof(Vector3d));

                                physicsEditCard.SetGridProperties(props); // ok if props array is null

                                // assign event handlers
                                physicsEditCard.OnPropertyValueChanged += OnBoxColliderPropertyValueChanged;
                                physicsEditCard.ControlClosed += OnPhysicsNodeRemoved;


                            }
                            break;

                        case "SphereCollider":
                            {
                                RigidBodyEditCard physicsEditCard = new RigidBodyEditCard(nodeID, entityID);
                                physicsEditCard.Text = nodeType;

                                superTabControlPanel9.Controls.Add(physicsEditCard);

                                // TODO: how do i assign multiple box colliders to an entity such as one for elevator floor and
                                // another for elevator entrance/exit trigger zone?  I think i decided i would use
                                // multiple entities... trigger zone being invisible entity
                                // TODO: we need to filter some of these properties like "id", "name", etc.
                                // that's why its probably better to just gather the properties we know we want
                                // one at a time and build the properspec array and pass it to the card.
                                //Settings.PropertySpec[] properties = mHost.Node_GetProperties (entityID);
                                Settings.PropertySpec[] props = new Settings.PropertySpec[3];
                                string category = "misc";

                                object value = mHost.Node_GetProperty(nodeID, "trigger");
                                props[0] = new Settings.PropertySpec("trigger", typeof(bool), category, (object)value, typeof(bool));

                                value = mHost.Node_GetProperty(nodeID, "center");
                                props[1] = new Settings.PropertySpec("center", typeof(Vector3d), category, (object)value, typeof(Vector3d));

                                value = mHost.Node_GetProperty(nodeID, "radius");
                                props[2] = new Settings.PropertySpec("radius", typeof(double), category, (object)value, typeof(double));

                                physicsEditCard.SetGridProperties(props); // ok if props array is null

                                // assign event handlers
                                physicsEditCard.OnPropertyValueChanged += OnSphereColliderPropertyValueChanged;
                                physicsEditCard.ControlClosed += OnPhysicsNodeRemoved;

                            }
                            break;

                        case "CapsuleCollider":
                            {
                                RigidBodyEditCard physicsEditCard = new RigidBodyEditCard(nodeID, entityID);
                                physicsEditCard.Text = nodeType;

                                superTabControlPanel9.Controls.Add(physicsEditCard);

                                // TODO: how do i assign multiple box colliders to an entity such as one for elevator floor and
                                // another for elevator entrance/exit trigger zone?  I think i decided i would use
                                // multiple entities... trigger zone being invisible entity
                                // TODO: we need to filter some of these properties like "id", "name", etc.
                                // that's why its probably better to just gather the properties we know we want
                                // one at a time and build the properspec array and pass it to the card.
                                //Settings.PropertySpec[] properties = mHost.Node_GetProperties (entityID);
                                Settings.PropertySpec[] props = new Settings.PropertySpec[5];
                                string category = "misc";

                                object value = mHost.Node_GetProperty(nodeID, "trigger");
                                props[0] = new Settings.PropertySpec("trigger", typeof(bool), category, (object)value, typeof(bool));

                                value = mHost.Node_GetProperty(nodeID, "radius");
                                props[1] = new Settings.PropertySpec("radius", typeof(double), category, (object)value, typeof(double));

                                value = mHost.Node_GetProperty(nodeID, "height");
                                props[2] = new Settings.PropertySpec("height", typeof(double), category, (object)value, typeof(double));

                                value = mHost.Node_GetProperty(nodeID, "center");
                                props[3] = new Settings.PropertySpec("center", typeof(Vector3d), category, (object)value, typeof(Vector3d));

                                value = mHost.Node_GetProperty(nodeID, "direction");
                                props[4] = new Settings.PropertySpec("direction", typeof(Vector3d), category, (object)value, typeof(Vector3d));

                                physicsEditCard.SetGridProperties(props); // ok if props array is null

                                // assign event handlers
                                physicsEditCard.OnPropertyValueChanged += OnCapsuleColliderPropertyValueChanged;
                                physicsEditCard.ControlClosed += OnPhysicsNodeRemoved;
                                                                            
                            }
                            break;
                    }
                }
                AutoArrangePanelCards(superTabControlPanel9);
            }
        }

        private void OnRigidBodyPropertyValueChanged(string nodeID, Settings.PropertySpec property)
        {

            switch (property.Name)
            {
                case "mass":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(double),  property.DefaultValue);
                    break;
                case "static":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(bool), property.DefaultValue);
                    break;
                case "drag":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(double), property.DefaultValue);
                    break;
                case "kinematic":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(bool), property.DefaultValue);
                    break;
            }
        }

        private void OnBoxColliderPropertyValueChanged(string nodeID, Settings.PropertySpec property)
        {

            switch (property.Name)
            {
                case "trigger":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(bool), property.DefaultValue);
                    break;
                case "size":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(Vector3d), property.DefaultValue);
                    break;
                case "center":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(Vector3d), property.DefaultValue);
                    break;
                default:
                    break;
            }
        }

        private void OnSphereColliderPropertyValueChanged(string nodeID, Settings.PropertySpec property)
        {
            switch (property.Name)
            {
                case "trigger":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(bool), property.DefaultValue);
                    break;
                case "radius":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(double), property.DefaultValue);
                    break;
                case "center":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(Vector3d), property.DefaultValue);
                    break;
                default:
                    break;
            }
        }


        private void OnCapsuleColliderPropertyValueChanged(string nodeID, Settings.PropertySpec property)
        {
            switch (property.Name)
            {
                case "trigger":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(bool), property.DefaultValue);
                    break;
                case "radius":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(double), property.DefaultValue);
                    break;
                case "height":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(double), property.DefaultValue);
                    break;
                case "center":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(Vector3d), property.DefaultValue);
                    break;
                case "direction":
                    mHost.Node_ChangeProperty(nodeID, property.Name, typeof(Vector3d), property.DefaultValue);
                    break;
                default:
                    break;
            }
        }

        private void OnPhysicsNodeRemoved(object sender, EventArgs e)
        {
            if (sender is RigidBodyEditCard)
            {
                RigidBodyEditCard card = (RigidBodyEditCard)sender;

                foreach (Control c in superTabControlPanel9.Controls)
                {
                    if (c == card)
                    {
                        card.ControlClosed -= OnPhysicsNodeRemoved;

                        mHost.Node_Remove(card.TargetID, card.ParentID);

                        superTabControlPanel9.Controls.Remove(c);
                        c.Dispose(); // override dispose to remove events?
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// To avoid "Error creating window handle" error, our Notecards must be properly
        /// removed.
        /// </summary>
        /// <param name="panel"></param>
        protected void ClearPhysicsCardsPanel(DevComponents.DotNetBar.SuperTabControlPanel panel)
        {
            if (panel.Controls != null && panel.Controls.Count > 0)
            {
                foreach (Control c in panel.Controls)
                {
                    c.Dispose(); // override dispose to remove events?
                }
            }
            panel.Controls.Clear();
        }
    }
}
