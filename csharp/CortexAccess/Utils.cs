using System;

namespace CortexAccess
{
    public class Utils
    {
        public static Int64 GetEpochTimeNow()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            Int64 timeSinceEpoch = (Int64)t.TotalMilliseconds;
            return timeSinceEpoch;

        }
        public static string GenerateUuidProfileName(string prefix)
        {
            return prefix + "-" + GetEpochTimeNow();
        }
    }
}
