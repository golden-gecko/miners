namespace Miners
{
    /// <summary>
    /// Represents counter. Counter updates its value by one until it reaches maximum value and then stops or resets.
    /// </summary>
    public class Counter
    {
        protected uint maximal = 0;
        protected uint current = 0;

        /// <summary>
        /// If set to true, timer will reset itself after reaching maximal value.
        /// </summary>
        protected bool autoReset = true;

        public uint Maximal
        {
            get { return maximal; }
            set { maximal = value; }
        }

        public uint Current
        {
            get { return current; }
            set { current = value; }
        }

        public bool Completed
        {
            get { return Current >= Maximal; }
        }

        public Counter(uint max, uint current = 0, bool autoReset = true)
        {
            this.maximal = max;
            this.current = current;
            this.autoReset = autoReset;
        }

        public virtual bool Update(uint value)
        {
            current += value;

            if (maximal < current)
            {
                if (autoReset)
                {
                    current = 0;
                }

                return true;
            }

            return false;
        }

        public virtual void Reset()
        {
            current = 0;
        }
    }
}
