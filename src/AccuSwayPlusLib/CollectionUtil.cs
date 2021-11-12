using System;
using System.Collections.ObjectModel;

namespace AccuSwayPlusLib
{
    class CollectionUtil
    {
        #nullable enable
        public static T? Find<T>(ReadOnlyCollection<T> coll, Predicate<T> match) {
            foreach(T elem in coll) {
                if(match(elem)) { return elem; }
            }
            return default(T);
        }

        public static bool Some<T>(ReadOnlyCollection<T> coll, Predicate<T> match)
        {
            foreach (T elem in coll)
            {
                if(match(elem)) { return true; }
            }
            return false;
        }
    }
}