using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace HitachiProtocol {
   public class HPStatus {
      private string[] description;
      private Color[] severity;
      private StateChange state;
      private Color stateColor;
      private int nACKs;
      private int nNAKs;
      private int ID;
      private string printerResponse;
      public int ACKs { get { return nACKs; } set { nACKs = value; } }
      public int NAKs { get { return nNAKs; } set { nNAKs = value; } }
      public StateChange State {
         get { return state; }
         set {
            switch (value) {
               case StateChange.Initializing:
               case StateChange.Connecting:
               case StateChange.ConnectFailed:
               case StateChange.Disconnected:
               case StateChange.TimeoutAbort:
               case StateChange.OffLine:
                  ResetDescriptions();
                  break;
            }
            if (value != StateChange.UpdateACKNAK) {
               state = value;
            }
         }
      }
      public string Response { get { return printerResponse; } set { printerResponse = value; } }

      internal HPStatus(int ID) {
         this.ID = ID;
         description = new string[4] { "N/A", "N/A", "N/A", "N/A" };
         severity = new Color[4] { Color.Gray, Color.Gray, Color.Gray, Color.Gray };
      }

      internal string GetDescription(StatusAreas whichArea) {
         return description[(int)whichArea];
      }

      internal void SetDescription(StatusAreas whichArea, string description) {
         this.description[(int)whichArea] = description;
      }

      internal void ResetDescriptions() {
         this.description[(int)StatusAreas.Connection] = "N/A";
         this.description[(int)StatusAreas.Reception] = "N/A";
         this.description[(int)StatusAreas.Operation] = "N/A";
         this.description[(int)StatusAreas.Alarm] = "N/A";
      }

      internal Color GetSeverity(StatusAreas whichArea) {
         return severity[(int)whichArea];
      }

      internal void SetSeverity(StatusAreas whichArea, Color severity) {
         this.severity[(int)whichArea] = severity;
      }

      internal void SetAllSeverity(Color severity) {
         this.severity[(int)StatusAreas.Connection] = severity;
         this.severity[(int)StatusAreas.Reception] = severity;
         this.severity[(int)StatusAreas.Operation] = severity;
         this.severity[(int)StatusAreas.Alarm] = severity;
      }

      internal void SetCounts(int nACKs, int nNAKs) {
         this.nACKs = nACKs;
         this.nNAKs = nNAKs;
      }

      internal Color GetMergedSeverity() {
         Color result = Color.LightGreen;
         if (state == StateChange.OffLine || state == StateChange.Disconnected) {
            result = Color.LightGray;
         } else {
            for (int i = 0; i < this.severity.Length; i++) {
               if (this.severity[i].ToArgb() != Color.LightGreen.ToArgb()) {
                  if (result == Color.Pink || this.severity[i] == Color.Pink) {
                     result = Color.Pink;
                  } else {
                     result = Color.Yellow;
                  }
               }
            }
            if (nNAKs > 0) {
               result = Color.Pink;
            }
         }
         return result;
      }

      public string PrinterStatusLine {
         get {
            string statusLine;
            string connection = GetDescription(StatusAreas.Connection);
            string reception = GetDescription(StatusAreas.Reception);
            string operation = GetDescription(StatusAreas.Operation);
            string alarm = GetDescription(StatusAreas.Alarm);
            string nACK = $"ACKs {this.nACKs}";
            string nNAK = $"NAKs {this.nNAKs}";
            string state = GetState();
            statusLine = $"{state} / {connection} / {reception} / {operation} / {alarm} /  {nACK} / {nNAK}";
            return statusLine;
         }
      }

      private string GetState() {
         string result;
         switch (state) {
            case StateChange.TimeoutRetrying:
               result = "I/O Timeout(Retrying)";
               break;
            case StateChange.TimeoutAbort:
               result = "I/O Timeout(Aborted)";
               break;
            case StateChange.OffLine:
               result = "Off Line";
               break;
            default:
               result = Enum.GetName(typeof(StateChange), state);
               break;
         }
         return result;
      }

   }

}