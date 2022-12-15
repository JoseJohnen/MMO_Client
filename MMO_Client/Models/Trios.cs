using System;

namespace MMO_Client.Code.Models
{
    [Serializable]
    [Stride.Core.DataContract]
    public class Trios<T1, T2, T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public Trios(T1 item1, T2 item2, T3 item3)
        {
            this.Item1 = item1;
            this.Item2 = item2;
            this.Item3 = item3;
        }

        #region ForEach Compatibility
        /*public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()   => GetEnumerator();*/
        #endregion
    }
}
