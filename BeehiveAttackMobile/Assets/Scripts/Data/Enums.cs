

namespace Data
{
    namespace Enums
    {
        public enum BeePersonality
        {
            worker,
            attacker,
            queen
        }

        public enum BeeState
        {
            working,
            defence,
            attack,
            returnToHive
        }

        public enum NectarControllerType
        {
            collector,
            theif,
            distributer,
            hub,
            none
        }

        public enum DetectionState
        {
            noIntruder,
            intruderDetected
        }

        public enum NectarStatus
        {
            idle, 
            increasing,
            decreasing,
            depleted,
            full
        }
    }
}
