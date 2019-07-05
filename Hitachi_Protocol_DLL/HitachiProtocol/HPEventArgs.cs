using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HitachiProtocol {
   public class HPEventArgs : IFormattable {

      #region Data Declarations and Properties

      public PrinterOps Op { get; set; }
      public int SubOp { get; set; }
      public string Message { get; set; }
      public int Item { get; set; }
      public string Font { get; set; }
      public int CharSize { get; set; }
      public int Page { get; set; }
      public int KbType { get; set; }
      public int nACKs { get; set; }
      public int nNAKs { get; set; }

      #endregion

      #region Constructors/Destructors

      public HPEventArgs()
         : this(PrinterOps.Nop, 0, 0, string.Empty, 0, 0, 0, string.Empty) { }

      public HPEventArgs(PrinterOps Op)
         : this(Op, 0, 0, string.Empty, 0, 0, 0, string.Empty) { }

      public HPEventArgs(PrinterOps Op, int SubOp)
         : this(Op, SubOp, 0, string.Empty, 0, 0, 0, string.Empty) { }

      public HPEventArgs(PrinterOps Op, int SubOp, string Message)
         : this(Op, SubOp, 0, string.Empty, 0, 0, 0, Message) { }

      public HPEventArgs(PrinterOps Op, int SubOp, int Item, string Message)
         : this(Op, SubOp, Item, string.Empty, 0, 0, 0, Message) { }

      public HPEventArgs(PrinterOps Op, string Message)
         : this(Op, 0, 0, string.Empty, 0, 0, 0, Message) { }

      public HPEventArgs(string Message)
         : this(PrinterOps.Nop, 0, 0, string.Empty, 0, 0, 0, Message) { }

      public HPEventArgs(PrinterOps Op, int SubOp, int Item, string Font, int CharSize, int Page, int KbType, string Message) {
         this.Op = Op;
         this.SubOp = SubOp;
         this.Item = Item;
         this.Font = Font;
         this.CharSize = CharSize;
         this.Page = Page;
         this.KbType = KbType;
         this.Message = Message;
      }

      public string ToString(string format, IFormatProvider provider) {
         string result = "";
         switch (format.ToLower()) {
            case "message":
               for (int i = 0; i < Message.Length; i++) {
                  char c = Message[i];
                  if (c < 0x20 || c > 0x7f) {
                     result += "<" + ((int)c).ToString("00") + ">";
                  } else {
                     result += Message.Substring(i, 1);
                  }
               }
               break;
         }
         return result;
      }

      #endregion

   }

}
