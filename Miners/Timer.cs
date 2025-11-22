namespace Miners
{
    /// <summary>
    /// Represents counter. Counter updates its value by given amount it reaches maximum value and then stops or resets.
    /// </summary>
    public class Timer
    {
        protected float maximal = 0.0f;
        protected float current = 0.0f;

        /// <summary>
        /// If set to true, timer will reset itself after reaching maximal value.
        /// </summary>
        protected bool autoReset = true;

        public float Maximal
        {
            get { return maximal; }
            set { maximal = value; }
        }

        public float Current
        {
            get { return current; }
            set { current = value; }
        }

        public bool Completed
        {
            get { return Current >= Maximal; }
            set { Current = Maximal; }
        }

        public Timer(float max, float current = 0.0f, bool autoReset = true)
        {
            this.maximal = max;
            this.current = current;
            this.autoReset = autoReset;
        }

        public virtual bool Update(float time)
        {
            current += time;

            if (maximal < current)
            {
                if (autoReset)
                {
                    current = 0.0f;
                }

                return true;
            }

            return false;
        }

        public virtual void Reset()
        {
            current = 0.0f;
        }
    }
}
