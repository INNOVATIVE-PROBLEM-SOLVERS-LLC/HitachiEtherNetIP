using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HitachiProtocol {

   public class HPRequest {

      #region Data Declarations

      //
      // Request data
      public PrinterOps Op = PrinterOps.Nop;
      public int SubOp = 0;
      public int Item = 0;

      public string Data1 = string.Empty;
      public string Data2 = string.Empty;
      public string Data3 = string.Empty;
      public string Data4 = string.Empty;
      public string Data5 = string.Empty;
      public string Data6 = string.Empty;
      public string Data7 = string.Empty;
      public string Data8 = string.Empty;
      public string Data9 = string.Empty;
      public string Data10 = string.Empty;
      public string Data11 = string.Empty;
      public string Data12 = string.Empty;
      public int CharSize = 0;
      public int Page = 0;
      public int KbType = 0;
      public int RcvLength = 0;
      public int Retries = 0;
      public int xCoord = 0;
      public int yCoord = 0;
      //public Message.FormatSetup Format = Message.FormatSetup.Individual;
      public int BlockNo = 0;
      public int TimedDelay = 0;
      public bool ExpectTextResponse= false;

      #endregion

      #region Constructors/Destructors

      public HPRequest() {

      }

      ~HPRequest() {

      }

      public void Clear() {
       Data1 = string.Empty;
       Data2 = string.Empty;
       Data3 = string.Empty;
       Data4 = string.Empty;
       Data5 = string.Empty;
       Data6 = string.Empty;
       Data7 = string.Empty;
       Data8 = string.Empty;
       Data9 = string.Empty;
       Data10 = string.Empty;
       Data11 = string.Empty;
       Data12 = string.Empty;
   }

   #endregion

}

}