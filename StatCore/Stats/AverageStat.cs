﻿using System;

namespace StatCore.Stats
{
    public class AverageStat<TTarget> : IStat<TTarget, double>
    {
        private AverageValue value;
        private readonly Func<TTarget, double> selector;
        public AverageStat(Func<TTarget, double> selector)
        {
            value = new AverageValue();
            this.selector = selector;
        }

        public void Add(TTarget item)
        {
            lock(value)
                value += selector(item);
        }

        public void Delete(TTarget item)
        {
            lock(value)
                value -= selector(item);
        }

        public double Value => value.Value;
        public bool IsEmpty => value.Count == 0;
    }
}