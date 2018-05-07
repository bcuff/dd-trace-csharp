﻿using System;

namespace DataDog.Tracing
{
    internal static class Util
    {
        [ThreadStatic]
        static Random _random1;

        [ThreadStatic]
        static Random _random2;

        static Random Random1 => _random1 ?? (_random1 = new Random(Guid.NewGuid().GetHashCode()));
        static Random Random2 => _random2 ?? (_random2 = new Random(Guid.NewGuid().GetHashCode()));

        public static long NewTraceId() => (long)(Random1.Next() << 32) | (long)Random2.Next();

        public static long NewSpanId() => (long)(Random1.Next() << 32) | (long)Random2.Next();

        static readonly long EpochTicks = new DateTime(1970, 1, 1).Ticks;

        public static TimeSpan FromNanoseconds(long nanoseconds)
        {
            return new TimeSpan(nanoseconds / 100);
        }

        public static long GetTimestamp() => (DateTime.UtcNow.Ticks - EpochTicks) * 100; // 100 nanoseconds in a tick
    }
}
