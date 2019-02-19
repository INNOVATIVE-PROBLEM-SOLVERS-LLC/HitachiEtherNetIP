using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HitachiEIP {

   public class AttrData {

      #region Properties and Constructor

      public byte Val { get; set; } = 0;             // The Attribute (Makes the tables easier to read)
      public bool HasSet { get; set; } = false;      // Supports a Set Request
      public bool HasGet { get; set; } = false;      // Supports a Get Request
      public bool HasService { get; set; } = false;  // Supports a Service Request
      public int Order { get; set; } = 0;            // Sort Order if Alpha Sort is requested
      public bool Ignore { get; set; } = false;      // Indicates that the request will hang printer

      // Four views of the printer data
      public Prop Data { get; set; }     // As it appears in the printer
      public Prop Get { get; set; }      // Data to be passed on a Get Request
      public Prop Set { get; set; }      // Data to be passed on a Set Request
      public Prop Service { get; set; }  // Data to be passed on a Service Request

      // A description of the data from four points of view.
      public AttrData(byte Val, GSS acc, bool Ignore, int Order, Prop Data, Prop Data2 = null) {
         this.Val = Val;
         this.HasSet = acc == GSS.Set || acc == GSS.GetSet;
         this.HasGet = acc == GSS.Get || acc == GSS.GetSet;
         this.HasService = acc == GSS.Service;
         this.Ignore = Ignore;
         this.Order = Order;

         // This is what the data looks like in the printer
         this.Data = Data;
         this.Set = Data;

         // Is there an extra property?
         if (Data2 != null) {
            if (HasGet) {
               // Get sometimes passes parameters
               this.Get = Data2;
            } else if (HasService) {
               // Service sometimes passes parameters
               this.Service = Data2;
            }
         } else {
            // Only one parameter? Get and service pass no data
            this.Service = this.Get = new Prop(0, DataFormats.Decimal, 0, 0);
         }
      }

      #endregion

   }

   // Look up AttrData using Class and Attribute
   public class Dictionary<TKey1, TKey2, TValue>
      : Dictionary<Tuple<TKey1, TKey2>, TValue>, IDictionary<Tuple<TKey1, TKey2>, TValue> {

      #region Constructor and methods

      // Convert the Class and Attribute to a Tuple for get/set
      public TValue this[TKey1 key1, TKey2 key2] {
         get { return base[Tuple.Create(key1, key2)]; }
         set { base[Tuple.Create(key1, key2)] = value; }
      }

      // Convert the Class and Attribute to a Tuple for Add
      public void Add(TKey1 key1, TKey2 key2, TValue value) {
         base.Add(Tuple.Create(key1, key2), value);
      }

      #endregion

   }

   public class Prop {

      #region Constructors, properties and methods

      public int Len { get; set; }
      public DataFormats Fmt { get; set; }
      public long Min { get; set; }
      public long Max { get; set; }
      public fmtDD DropDown { get; set; }

      public Prop(int Len, DataFormats Fmt, long Min, long Max, fmtDD DropDown = fmtDD.None) {
         this.Len = Len;
         this.Fmt = Fmt;
         this.Min = Min;
         this.Max = Max;
         this.DropDown = DropDown;
      }

      #endregion

   }

}
