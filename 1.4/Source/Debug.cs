﻿namespace XenotypeRandomizer
{
    public static class Debug
    {
        public static void Log(string message)
        {
#if DEBUG
            Verse.Log.Message($"[{XenotypeRandomizerMod.PACKAGE_NAME}] {message}");
#endif
        }
    }
}
