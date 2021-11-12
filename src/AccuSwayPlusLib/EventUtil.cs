using System;
using System.Collections.Generic;



namespace AccuSwayPlusLib
{
    public class EventAggregator<T>
    {
        public List<T> events { get; private set; }

        public EventAggregator() {
            this.events = new List<T>();
        }

        public void OnEvent(Object sender, T args)
        {
            this.events.Add(args);
        }
    }
}
