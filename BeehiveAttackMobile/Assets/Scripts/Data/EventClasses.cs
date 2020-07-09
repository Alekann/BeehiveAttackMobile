using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Data
{
    namespace EventClasses
    {
        /// <summary>
        /// Class to pass a transform object each time the event is involked.
        /// </summary>
        [System.Serializable]
        public class TransformUnityEvent : UnityEvent<Transform> { }

    }
}
