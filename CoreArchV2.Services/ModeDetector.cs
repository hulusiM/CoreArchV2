namespace CoreArchV2.Services
{
    public class ModeDetector
    {
        /// <summary>
        /// Gets a value indicating whether the assembly was built in debug mode.
        /// </summary>
        public virtual bool IsDebug
        {
            get
            {
                bool isDebug = false;

#if (DEBUG)
                isDebug = true;
#else
                                isDebug = false;
#endif

                return isDebug;
            }
        }
    }
}
