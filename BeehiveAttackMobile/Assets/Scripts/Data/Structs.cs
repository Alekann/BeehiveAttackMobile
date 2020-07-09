using System.Collections;
using System.Collections.Generic;
using Data.Enums;
using UnityEngine;

namespace Data
{
    namespace Stucts
    {
        [System.Serializable]
        public struct NectarReceiver
        {
            public NectarControllerType receiveFromType;
            public float receiveRate;
            public bool affectOwnersSupply;
            public bool affectsOthersSupply;

            [Range(1.0f, 30.0f)]
            public float multiplierForOthersRate;
        }

        [System.Serializable]
        public struct NectarSender
        {
            public NectarControllerType sendToType;
            public float sendRate;
            public bool affectsOwnersSupply;
        }
    }
}