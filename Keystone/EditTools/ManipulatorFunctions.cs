using System;
using System.Collections.Generic;
using Keystone.Types;
using Keystone.Cameras;
using System.Drawing;
using Keystone.Elements;


namespace Keystone.EditTools
{

    /// <summary>
    /// Defines the directions of an axis
    /// </summary>
    [Flags]
    public enum AxisDirections
    {
        Positive = (0x1 << 0),      // Positive direction of an axis
        Negative = (0x1 << 1),      // Negative direction of an axis

        All = Positive | Negative   // Both positive and negative directions
    }

    /// <summary>
    /// Defines the transformation mode of a manipulator
    /// </summary>
    [Flags]
    public enum TransformationMode
    {
        None,                               // No manipulation mode

        TranslationAxis = (0x1 << 0),        // Manipulating the position of an object along an axis
        TranslationPlane = (0x1 << 1),       // Manipulating the position of an object along a plane
        Rotation = (0x1 << 2),               // Manipulating the orientation of an object around an axis
        ScaleAxis = (0x1 << 3),              // Manipulating the scale of an object on an axis
        ScalePlane = (0x1 << 4),             // Manipulating the scale of an object on a plane (two-axes)
        ScaleUniform = (0x1 << 5)            // Manipulating the scale of an object uniformly
    }

 

    public delegate void ManipFunction(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace);
    public delegate void RotationFunction ();

    // TODO: Very interesting
    // Snapping for position, rotation and scaling is easy to do
    // by simply passing in a "Precision" value.  If precious > 0.0
    // then every change is rounded to the +/- precision increment.  
    // That is essentially snapping.
    public class ManipFunctions2 : Dictionary<TransformationMode, Dictionary<AxisFlags, ManipFunction>>
    {

        public ManipFunctions2()
        {
            foreach (TransformationMode mode in System.Enum.GetValues(typeof(TransformationMode)))
                this[mode] = new Dictionary<AxisFlags, ManipFunction>();


            //CongigurePositioningManipulator();
            ConfigureScalingManipulator();
            ConfigureRotatingManipulator();
        }


        #region Delegates Configuration
//        private void CongigurePositioningManipulator()
//        {
//
//            this[TransformationMode.TranslationPlane][AxisFlags.X | AxisFlags.Y]
//            = this[TransformationMode.TranslationPlane][AxisFlags.X | AxisFlags.Z]
//            = this[TransformationMode.TranslationPlane][AxisFlags.Y | AxisFlags.Z]
//                = delegate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
//                {
//                    if (target == null) return;
//                    // TODO: How do i deal with the position of the camera for orth views where
//                    // the Y location is like the near plane... or is it a case of the near plane being like
//                    // 10001 and our plane at 10000 so we dont even clip the near plane?
//                    // TODO: i think part of the problem is our UnProject and Project returns camera space
//                    // values, and by using .Context.Position we are using world space values.  This is why
//                    // passing in the difference position below of target and position will result in
//                    // origin plane from within GetPlane() since it also translates the resulting plane
//                    // by the difference. 
//                    // get the plane representing the two selected axes
//
//                    // TODO: passing in Position results in I think, a 0,0,0 plane being created...
//                    // within GetPlane()... so... it works for now but keep an eye out for this.... 
//                    Plane p = Plane.GetPlane(selectedAxes);
//                    
//                    //Vector3d translation = target.Translation - cameraPosition;
//                    //Plane p = new Plane(target.Translation, normal);
//
//                    ////if (vectorSpace == VectorSpace.Local) // local to the gizmo?
//                    ////    p.Transform(new Matrix(target.Rotation) * Matrix.Translation(translation));
//                    ////else
//                    //    p.Transform(Matrix.Translation(translation));
//
//
//
//                    double hitDistanceStart, hitDistanceEnd;
//                    Ray start_ray, end_ray;
//
//                    
//                    // cast rays into the scene from the mouse start and end points and
//                    // intersect the pick rays with the dual axis plane we want to move along
//
//                    // if either of the intersections is invalid then bail out as it would
//                    // be impossible to calculate the difference
//                    if (!currentViewport.Pick_PlaneStartEndRays(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
//                        mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
//                        return;
//
//                    // obtain the intersection points of each ray with the plane
//                    Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
//                    Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);
//
//                    // calculate the difference between the intersection points
//                    Vector3d difference = end_pos - start_pos;
//                    System.Diagnostics.Trace.WriteLine(difference.ToString());
//                    // TODO: commenting out the frustum test for now
//                    //// obtain the current view frustum using the camera's view and projection matrices
//                    //BoundingFrustum frust = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
//
//                    //// if the new translation is within the current camera frustum, then add the difference
//                    //// to the current transformation's translation component
//                    //if (frust.Contains(mTransform.Translation + diff) == ContainmentType.Contains)
//
//                    // note: the target is the actual entity we want to move, NOT the control
//                    // because the control is parented to that entity and so will maintain it's relative
//                    // position as the target entity moves
//                    target.Translation += difference;
//                };
//
//            this[TransformationMode.TranslationAxis][AxisFlags.X]
//                = this[TransformationMode.TranslationAxis][AxisFlags.Y]
//                = this[TransformationMode.TranslationAxis][AxisFlags.Z]
//                = delegate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
//                {
//                    if (target == null) return;
//
//                    // get the unit version of the selected axis
//                    // NOTE: The unit axis accrues values from muliple axis if applicable such as when
//                    // moving by both X and Z plane at same time or any other plane.
//                    Vector3d axis = Axis.GetUnitAxis(target.Rotation, selectedAxes, vectorSpace);
//
//                    // we need to project using the translation component of the current
//                    // ITransform in order to obtain a projected unit axis originating
//                    // from the transform's position
//                    Vector3d start_position;
//                    Vector3d end_position;
//                    Vector3d screen_direction;
//                    Vector3d difference;
//
//                    // project the origin onto the screen at the transform's position
//                    start_position = currentViewport.Project(Vector3d.Origin() + target.Translation - currentViewport.Context.Position);
//                    // project the unit axis onto the screen at the transform's position
//                    end_position = currentViewport.Project(axis + target.Translation - currentViewport.Context.Position);
//                    start_position.z = 0; // projected values are 2D, so let's clean up the z to be perfect 0
//                    end_position.z = 0;
//
//                    // calculate the normalized direction vector of the unit axis in screen space
//                    screen_direction = Vector3d.Normalize(end_position - start_position);
//                    // calculate the projected mouse delta along the screen direction vector
//                    Point mouseDelta = new Point();
//                    mouseDelta.X = mouseStart.X - mouseEnd.X;
//                    mouseDelta.Y = mouseStart.Y - mouseEnd.Y;
//                    end_position = start_position +
//                        (screen_direction * (Vector3d.DotProduct(new Vector3d(mouseDelta.X, mouseDelta.Y, 0), screen_direction)));
//
//
//                    double desiredDistance = (target.Translation - currentViewport.Context.Position).Length;
//
//                    start_position = currentViewport.UnProject(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
//                        start_position.x, start_position.y, desiredDistance);
//                    end_position = currentViewport.UnProject(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
//                        end_position.x, end_position.y, desiredDistance);
//                
//                    // calculate the difference vector between the world space start and end points
//                    difference = end_position - start_position;
//
//                    // note: the target is the actual entity we want to move, NOT the control
//                    // because the control is parented to that entity and so will maintain it's relative
//                    // position as the target entity moves
//                    target.Translation += difference;
//                    target.LatestStepTranslation += difference;
//                };
//        }

        // http://www.youtube.com/watch?v=QI-7vSFB9j4
        private void ConfigureRotatingManipulator()
        {
            this[TransformationMode.Rotation][AxisFlags.X]
                = this[TransformationMode.Rotation][AxisFlags.Y]
                = this[TransformationMode.Rotation][AxisFlags.Z]
                = delegate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
                {
                    // get the plane perpendicular to the rotation axis, transformed by
                    // the current world matrix (or in our case camera space matrix)
                    Vector3d distanceToTarget = target.Translation - currentViewport.Context.Position;
                    // TODO: test the following version that uses distanceToTarget instead of .Context.Position because this also fixed orthographic picking
                    // TODO: maybe it would be ok for orthographic rotations, but not free perspective
                   // distanceToTarget = Vector3d.Zero(); // target.Translation;
                    //Plane p = GetPlane(distanceToTarget, target, selectedAxes, vectorSpace);
                    // TODO: for interior deck component, this plane height is flat our wrong.
                    //       in our current test it should be at height -3.5 which is height
                    // of our deck 6 on morena smuggler but it's obviously at camera position
                    //   Why are we not simply passing in the plane?!  We should not be dealing with target
                    //   positions and context positions at all i dont think.
                    // TODO: do we rather want to get plane at origin?
                    Plane p = Plane.GetPlane(currentViewport.Context.Position, selectedAxes);
                    //Vector3d translation = target.Translation - cameraPosition;
                    //Plane p = new Plane(target.Translation, normal);

                    ////if (vectorSpace == VectorSpace.Local) // local to the gizmo?
                    ////    p.Transform(new Matrix(target.Rotation) * Matrix.Translation(translation));
                    ////else
                    //    p.Transform(Matrix.Translation(translation));


                    double hitDistanceStart, hitDistanceEnd;
                    Ray start_ray, end_ray;
                    if (!currentViewport.Pick_PlaneStartEndRays(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
                        mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
                        return;

                    // calculate the intersection position of each ray on the plane
                    Vector3d start_position = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
                    Vector3d end_position = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

                    // get the direction vectors of the rotation origin to the start and end points
                    Vector3d origin_to_start = Vector3d.Normalize(start_position - distanceToTarget);
                    Vector3d origin_to_end = Vector3d.Normalize(end_position - distanceToTarget);

                    Vector3d rotation_axis = Axis.GetUnitAxis(target.Rotation, selectedAxes, vectorSpace);

                    // calculate cross products of the direction vectors with the rotation axis
                    Vector3d rot_cross_start = Vector3d.Normalize(Vector3d.CrossProduct(rotation_axis, origin_to_start));
                    Vector3d rot_cross_end = Vector3d.Normalize(Vector3d.CrossProduct(rotation_axis, origin_to_end));

                    // calculate the cross product of the above start and end cross products
                    Vector3d start_cross_end = Vector3d.Normalize(Vector3d.CrossProduct(rot_cross_start, rot_cross_end));

                    // dot the two direction vectors and get the arccos of the dot product to get
                    // the angle between them, then multiply it by the sign of the dot product
                    // of the derived cross product calculated above to obtain the direction
                    // by which we should rotate with the angle
                    double dot = Vector3d.DotProduct(origin_to_start, origin_to_end);
                    double sign = Math.Sign(Vector3d.DotProduct(rotation_axis, start_cross_end));
                    double scale = .02; // we are scaling because for some reason our angle is so tiny
                    if (sign < 0.0) scale *= -1;
                    double rotation_angle = (Math.Acos(dot) * sign) + scale;

                    // create a normalized quaternion representing the rotation from the start to end points
                    //Quaternion rot = Quaternion.Normalize(new Quaternion(rotation_axis, rotation_angle));
                    Quaternion rot = new Quaternion(rotation_axis, rotation_angle);

                    if (double.IsNaN(rot.X) || double.IsNaN(rot.Y) || double.IsNaN(rot.Z) || double.IsNaN(rot.W))
                        return;

                    // note: the target is the actual entity we want to rotate, NOT the control
                    // because the control is parented to that entity and so will maintain it's relative
                    // rotation as the target entity rotates
                    target.Rotation =  Quaternion.Normalize(rot * target.Rotation);

                    // TODO: the following is clearly missing something.... 
                    //if (mVectorSpace == VectorSpace.Local)
                    //    _control.Rotation = _target.Rotation;

                };
        }

        private void ConfigureScalingManipulator()
        {
            // all single-axis scaling will use the same manip function
            this[TransformationMode.ScaleAxis][AxisFlags.X]
                = this[TransformationMode.ScaleAxis][AxisFlags.Y]
                = this[TransformationMode.ScaleAxis][AxisFlags.Z]
                    = delegate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
                    {
                        if (target == null) return;

                        // get the axis for the component being scaled
                        Vector3d axis = Axis.GetUnitAxis(target.Rotation, selectedAxes, vectorSpace);

                        // get a translation matrix on which the projection of the above axis
                        // will be based
                        Matrix translation = Matrix.CreateTranslation(target.Translation - currentViewport.Context.Position);
                        translation = Matrix.Identity();

                        // project the axis into screen space
                        Vector3d p0 = currentViewport.Project(new Vector3d(), translation);
                        Vector3d p1 = currentViewport.Project(axis, translation);

                        // disregard the z component for 2D calculations
                        p0.z = p1.z = 0;

                        // Vector3d versions of the mouse input positions
                        Vector3d ps = new Vector3d(mouseStart.X, mouseStart.Y, 0);
                        Vector3d pe = new Vector3d(mouseEnd.X, mouseEnd.Y, 0);

                        // calculate the axis vector and vectors from the translation point
                        // to each of the mouse positions
                        Vector3d v0 = p1 - p0;
                        Vector3d vs = ps - p0;
                        Vector3d ve = pe - p0;

                        // project both mouse positions onto the axis vector and calculate
                        // their scalars
                        double proj_s = Math.Abs(Vector3d.DotProduct(vs, v0) / v0.Length);
                        double proj_e = Math.Abs(Vector3d.DotProduct(ve, v0) / v0.Length);

                        // find the ratio between the projected scalar values
                        Vector3d scale = target.Scale;
                        double ratio = (proj_e / proj_s);

                        // scale the appropriate axis by the ratio
                        switch (selectedAxes)
                        {
                            case AxisFlags.X:
                                scale.x *= ratio;
                                break;

                            case AxisFlags.Y:
                                scale.y *= ratio;
                                break;

                            case AxisFlags.Z:
                                scale.z *= ratio;
                                break;
                        }

                        // clamp each component of the new scale to a sane value
                        scale.x = Utilities.MathHelper.Clamp(scale.x, double.Epsilon, double.MaxValue);
                        scale.y = Utilities.MathHelper.Clamp(scale.y, double.Epsilon, double.MaxValue);
                        scale.z = Utilities.MathHelper.Clamp(scale.z, double.Epsilon, double.MaxValue);

                        // note: the target is the actual entity we want to scale, NOT the control
                        // because the control is parented to that entity and so will maintain it's relative
                        // scale as the target entity scales
                        target.Scale = scale;
                    };


            // all dual-axis scaling will use the same manip function
            this[TransformationMode.ScalePlane][AxisFlags.X | AxisFlags.Y]
                = this[TransformationMode.ScalePlane][AxisFlags.X | AxisFlags.Z]
                = this[TransformationMode.ScalePlane][AxisFlags.Y | AxisFlags.Z]
                    = delegate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
                    {
                        // get the plane that corresponds to the axes on which we are performing the scale
                        // TODO: do we rather want to getplane at origin?
                        Plane p = Plane.GetPlane(currentViewport.Context.Position, selectedAxes);
                        //Vector3d translation = target.Translation - cameraPosition;
                        //Plane p = new Plane(target.Translation, normal);

                        ////if (vectorSpace == VectorSpace.Local) // local to the gizmo?
                        ////    p.Transform(new Matrix(target.Rotation) * Matrix.Translation(translation));
                        ////else
                        //    p.Transform(Matrix.Translation(translation));


                        double hitDistanceStart, hitDistanceEnd;
                        Ray start_ray, end_ray;
                        if (!currentViewport.Pick_PlaneStartEndRays(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
                            mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
                            return;

                        // calculate the intersection points of each ray along the plane
                        Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
                        Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

                        // find the vectors from the transform's position to each intersection point
                        Vector3d start_to_pos = start_pos - target.Translation;
                        Vector3d end_to_pos = end_pos - target.Translation;



                        // get the lengths of both of these vectors and find the ratio between them
                        double start_len = start_to_pos.Length;
                        double end_len = end_to_pos.Length;

                        Vector3d scale = target.Scale;
                        double ratio = (start_len == 0)
                            ? (1)
                            : (end_len / start_len);

                        // scale the selected components by the ratio
                        if ((selectedAxes & AxisFlags.X) == AxisFlags.X)
                            scale.x *= ratio;
                        if ((selectedAxes & AxisFlags.Y) == AxisFlags.Y)
                            scale.y *= ratio;
                        if ((selectedAxes & AxisFlags.Z) == AxisFlags.Z)
                            scale.z *= ratio;

                        // clamp each component of the new scale to a sane value
                        scale.x = Utilities.MathHelper.Clamp(scale.x, double.Epsilon, double.MaxValue);
                        scale.y = Utilities.MathHelper.Clamp(scale.y, double.Epsilon, double.MaxValue);
                        scale.z = Utilities.MathHelper.Clamp(scale.z, double.Epsilon, double.MaxValue);

                        // note: the target is the actual entity we want to scale, NOT the control
                        // because the control is parented to that entity and so will maintain it's relative
                        // scale as the target entity scales
                        target.Scale = scale;
                    };

            // Uniform scaling
            this[TransformationMode.ScaleUniform][AxisFlags.X | AxisFlags.Y | AxisFlags.Z]
                    = delegate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
                    {
                        // get the direction of the transformation's position to the camera position
                        Vector3d pos_to_cam = currentViewport.Context.Position - target.Translation;
                        //= mCurrentViewport.Context.Camera.Inverse).Translation - _target.Translation;

                        // normalize the direction for use in plane construction
                        if (pos_to_cam.x != 0 || pos_to_cam.y != 0 || pos_to_cam.z != 0)
                            pos_to_cam.Normalize();

                        // create a plane with the normal calculated above that passes through
                        // the transform's position
                        Plane p = new Plane(pos_to_cam, 0);
                        p.Transform(Matrix.CreateTranslation(target.Translation));


                        double hitDistanceStart, hitDistanceEnd;
                        Ray start_ray, end_ray;
                        if (!currentViewport.Pick_PlaneStartEndRays(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
                            mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
                            return;

                        // calculate the intersection points of each ray along the plane
                        Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
                        Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

                        // find the vectors from the transform's position to each intersection point
                        Vector3d start_to_pos = start_pos - target.Translation;
                        Vector3d end_to_pos = end_pos - target.Translation;




                        // get the lengths of both of these vectors and find the ratio between them
                        double start_len = start_to_pos.Length;
                        double end_len = end_to_pos.Length;

                        Vector3d scale = target.Scale;
                        double ratio = (start_len == 0)
                            ? (1)
                            : (end_len / start_len);

                        // multiply the scale uniformly by the ratio of the start and end vector lengths
                        scale *= ratio;

                        // clamp each component of the new scale to a sane value
                        scale.x = Utilities.MathHelper.Clamp(scale.x, double.Epsilon, double.MaxValue);
                        scale.y = Utilities.MathHelper.Clamp(scale.y, double.Epsilon, double.MaxValue);
                        scale.z = Utilities.MathHelper.Clamp(scale.z, double.Epsilon, double.MaxValue);

                        // note: the target is the actual entity we want to scale, NOT the control
                        // because the control is parented to that entity and so will maintain it's relative
                        // scale as the target entity scales
                        target.Scale = scale;
                    };
        }
        #endregion
    }


    public class RotationFunctions 
    {

        /// <summary>
        /// Computes a FINAL translation that can be set directly on Transform.Translation without any multiplication.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="currentViewport"></param>
        /// <param name="mouseStart"></param>
        /// <param name="mouseEnd"></param>
        /// <param name="selectedAxes"></param>
        /// <param name="vectorSpace"></param>
        /// <returns></returns>
        public static Vector3d Position (Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
        {    	
	        if (target == null) throw new ArgumentNullException();
	        Vector3d position = target.Translation;
        	Vector3d difference; 
        	
        	if (selectedAxes == AxisFlags.X || selectedAxes == AxisFlags.Y || selectedAxes == AxisFlags.Z)
        	{
        		// get the unit version of the selected axis
                // NOTE: The unit axis accrues values from muliple axis if applicable such as when
                // moving by both X and Z plane at same time or any other plane.
                Vector3d axis = Axis.GetUnitAxis(target.Rotation, selectedAxes, vectorSpace);

                // we need to project using the translation component of the current
                // ITransform in order to obtain a projected unit axis originating
                // from the transform's position
                Vector3d start_position;
                Vector3d end_position;
                Vector3d screen_direction;
 
                // project the origin onto the screen at the transform's position
                start_position = currentViewport.Project(Vector3d.Zero() + target.Translation - currentViewport.Context.Position,
                                                        currentViewport.Context.Camera.View,
                                                        currentViewport.Context.Camera.Projection,
                                                        Matrix.Identity());
                // project the unit axis onto the screen at the transform's position
                end_position = currentViewport.Project(axis + target.Translation - currentViewport.Context.Position,
                                                        currentViewport.Context.Camera.View,
                                                        currentViewport.Context.Camera.Projection,
                                                        Matrix.Identity());
                // projected values are 2D, so let's clean up the z to be perfect 0
                start_position.z = 0; 
                end_position.z = 0;

                // calculate the normalized direction vector of the unit axis in screen space
                screen_direction = Vector3d.Normalize(end_position - start_position);
                // calculate the projected mouse delta along the screen direction vector
                Point mouseDelta = new Point();
                mouseDelta.X = mouseStart.X - mouseEnd.X;
                mouseDelta.Y = mouseStart.Y - mouseEnd.Y;
                end_position = start_position +
                    (screen_direction * (Vector3d.DotProduct(new Vector3d(mouseDelta.X, mouseDelta.Y, 0), screen_direction)));


                double desiredDistance = (target.Translation - currentViewport.Context.Position).Length;

                start_position = currentViewport.UnProject(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
                                                           start_position.x, start_position.y, desiredDistance);
                end_position = currentViewport.UnProject(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, 
                                                         end_position.x, end_position.y, desiredDistance);
            
                // calculate the difference vector between the world space start and end points
                difference = end_position - start_position;
                
                // ------------------------------------------ ALTERNATIVE
                //Matrix billboardMatrix = Matrix.CreateBillboardRotationMatrix(Vector3d.Up(), currentViewport.Context.LookAt);
                //// translate plane to the world space position of our waypoint marker
                //billboardMatrix.SetTranslation(target.Translation);

                //Plane p = Plane.GetPlane(AxisFlags.YX);
                //p.Transform(billboardMatrix);
                    
                //start_position = currentViewport.Context.Viewport.Pick_PlaneRayIntersection(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, currentViewport.Context.Position, p, mouseStart.X, mouseStart.Y);
                //end_position = currentViewport.Context.Viewport.Pick_PlaneRayIntersection(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection, currentViewport.Context.Position, p, mouseEnd.X, mouseEnd.Y);
                //difference = end_position - start_position;
        	}
			else 
			{
	            // TODO: How do i deal with the position of the camera for orth views where
	            // the Y location is like the near plane... or is it a case of the near plane being like
	            // 10001 and our plane at 10000 so we dont even clip the near plane?
	            // TODO: i think part of the problem is our UnProject and Project returns camera space
	            // values, and by using .Context.Position we are using world space values.  This is why
	            // passing in the difference position below of target and position will result in
	            // origin plane from within GetPlane() since it also translates the resulting plane
	            // by the difference. 
	            // get the plane representing the two selected axes
	
	            // TODO: we should provide a plane distance
	
	            // TODO: passing in Position results in I think, a 0,0,0 plane being created...
	            // within GetPlane()... so... it works for now but keep an eye out for this.... 
	            Plane p = Plane.GetPlane(selectedAxes);
	
	            Vector3d translation = target.Translation; // - currentViewport.Context.Position;
	            //Plane p = new Plane(target.Translation, normal);
	
	            ////if (vectorSpace == VectorSpace.Local) // local to the gizmo?
	            ////    p.Transform(new Matrix(target.Rotation) * Matrix.Translation(translation));
	            ////else
	                p.Transform(Matrix.CreateTranslation(translation));
	
	
	            double hitDistanceStart, hitDistanceEnd;
	            Ray start_ray, end_ray;
	
	
	            // cast rays into the scene from the mouse start and end points and
	            // intersect the pick rays with the dual axis plane we want to move along
	
	            // if either of the intersections is invalid then bail out as it would
	            // be impossible to calculate the difference
	            if (!currentViewport.Pick_PlaneStartEndRays(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection,
	                mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
	                return position;
	
	
	            // obtain the intersection points of each ray with the plane
	            Vector3d start_pos = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
	            Vector3d end_pos = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);
	
	            // calculate the difference between the intersection points
	            difference = end_pos - start_pos;
	            System.Diagnostics.Trace.WriteLine(difference.ToString());
	            // TODO: commenting out the frustum test for now
	            //// obtain the current view frustum using the camera's view and projection matrices
	            //BoundingFrustum frust = new BoundingFrustum(ViewMatrix * ProjectionMatrix);
	
	            //// if the new translation is within the current camera frustum, then add the difference
	            //// to the current transformation's translation component
	            //if (frust.Contains(mTransform.Translation + diff) == ContainmentType.Contains)
			}
			
            // note: the target is the actual entity we want to move, NOT the control
            // because the control is parented to that entity and so will maintain it's relative
            // position as the target entity moves
            position = difference;

            return position;
        }

        public static Quaternion RotateYAxis(Transform target, Point mouseStart, Point mouseEnd)
        {
            Quaternion result = target.Rotation;
            Vector3d angles = result.GetEulerAngles(false);

            double rate = 0.2d;
            angles.y += Math.Sin((mouseStart.X - mouseEnd.X) * rate); 

            // snap
            const double PIOVER4 = Math.PI / 4d;

            angles.y = Math.Round (angles.y / PIOVER4) * PIOVER4;
           // System.Diagnostics.Debug.WriteLine("RotateYAxis() - angle = " + angles.y.ToString());
            // todo: snap
            // todo: hide the mouse during operation
            // todo: ideally orient the "front" of the model to oppose walls
            // todo: update footprint
            // todo: need a way to know what is the "front" of an object and paint an arrow graphic.  We can just assume that the starting rotation facing the camera +Z represents the front
            result = new Quaternion(Vector3d.Up(), angles.y);
            return result;
        }

        /// <summary>
        /// Computes a FINAL rotation that can be set directly on Transform.Rotation without any multiplication.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="currentViewport"></param>
        /// <param name="mouseStart"></param>
        /// <param name="mouseEnd"></param>
        /// <param name="selectedAxes"></param>
        /// <param name="vectorSpace"></param>
        /// <returns></returns>
        public static Quaternion Rotate(Transform target, Viewport currentViewport, Point mouseStart, Point mouseEnd, AxisFlags selectedAxes, VectorSpace vectorSpace)
        {
            Quaternion result = target.Rotation; // initialize to starting rotation

            // get the plane perpendicular to the rotation axis, transformed by
            // the current world matrix (or in our case camera space matrix)
            Vector3d distanceToTarget = target.Translation  - currentViewport.Context.Position; // todo: i dont think this takes into account region's with different coordinate systems for Target
            
            // TODO: test the following version that uses distanceToTarget instead of .Context.Position because this also fixed orthographic picking
            // TODO: maybe it would be ok for orthographic rotations, but not free perspective
            // distanceToTarget = Vector3d.Zero(); // target.Translation;
            //Plane p = Plane.GetPlane(distanceToTarget, target, selectedAxes, vectorSpace);
            // TODO: for interior deck component, this plane height is flat our wrong.
            //       in our current test it should be at height -3.5 which is height
            // of our deck 6 on morena smuggler but it's obviously at camera position
            //   Why are we not simply passing in the plane?!  We should not be dealing with target
            //   positions and context positions at all i dont think.

            Plane p = Plane.GetPlane(selectedAxes);
            //Vector3d translation = target.Translation - currentViewport.Context.Position;
             p = new Plane(distanceToTarget, p.Normal);

            ////if (vectorSpace == VectorSpace.Local) // local to the gizmo?
            ////    p.Transform(new Matrix(target.Rotation) * Matrix.Translation(translation));
            ////else
            //    p.Transform(Matrix.Translation(translation));


            double hitDistanceStart, hitDistanceEnd;
            Ray start_ray, end_ray;
            if (!currentViewport.Pick_PlaneStartEndRays(currentViewport.Context.Camera.View, currentViewport.Context.Camera.Projection,
                mouseStart, mouseEnd, p, out start_ray, out end_ray, out hitDistanceStart, out hitDistanceEnd))
                return result;

            // calculate the intersection position of each ray on the plane
            Vector3d start_position = start_ray.Origin + (start_ray.Direction * hitDistanceStart);
            Vector3d end_position = end_ray.Origin + (end_ray.Direction * hitDistanceEnd);

            // get the direction vectors of the rotation origin to the start and end points
            Vector3d origin_to_start = Vector3d.Normalize(start_position - distanceToTarget);
            Vector3d origin_to_end = Vector3d.Normalize(end_position - distanceToTarget);

            Vector3d rotation_axis = Axis.GetUnitAxis(target.Rotation, selectedAxes, vectorSpace);

            // calculate cross products of the direction vectors with the rotation axis
            Vector3d rot_cross_start = Vector3d.Normalize(Vector3d.CrossProduct(rotation_axis, origin_to_start));
            Vector3d rot_cross_end = Vector3d.Normalize(Vector3d.CrossProduct(rotation_axis, origin_to_end));

            // calculate the cross product of the above start and end cross products
            Vector3d start_cross_end = Vector3d.Normalize(Vector3d.CrossProduct(rot_cross_start, rot_cross_end));

            // dot the two direction vectors and get the arccos of the dot product to get
            // the angle between them, then multiply it by the sign of the dot product
            // of the derived cross product calculated above to obtain the direction
            // by which we should rotate with the angle
            double dot = Vector3d.DotProduct(origin_to_start, origin_to_end);
            double sign = Math.Sign(Vector3d.DotProduct(rotation_axis, start_cross_end));
            double scale = .02; // we are scaling because for some reason our angle is so tiny
            if (sign < 0.0) scale *= -1;
            double rotation_angle = (Math.Acos(dot) * sign) + scale;

            // create a normalized quaternion representing the rotation from the start to end points
            //Quaternion rot = Quaternion.Normalize(new Quaternion(rotation_axis, rotation_angle));
            Quaternion rot = new Quaternion(rotation_axis, rotation_angle);

            if (double.IsNaN(rot.X) || double.IsNaN(rot.Y) || double.IsNaN(rot.Z) || double.IsNaN(rot.W))
                return result; 

            // note: the target is the actual entity we want to rotate, NOT the control
            // because the control is parented to that entity and so will maintain it's relative
            // rotation as the target entity rotates
            result = Quaternion.Normalize(rot * target.Rotation);

            // TODO: the following is clearly missing something.... 
            //if (mVectorSpace == VectorSpace.Local)
            //    _control.Rotation = _target.Rotation;

            return result;
        }
    }
}
