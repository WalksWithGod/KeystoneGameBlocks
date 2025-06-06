﻿using Keystone.Types;

namespace Steering
{
    /// <summary>
    /// A flow field which can be sampled at arbitrary locations
    /// </summary>
    public interface IFlowField
    {
        /// <summary>
        /// Sample the flow field at the given location
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        Vector3d Sample(Vector3d location);
    }
}
