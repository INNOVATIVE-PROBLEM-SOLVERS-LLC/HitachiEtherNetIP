using System;
using System.Collections.Generic;
using System.Drawing;

namespace HitachiProtocol {

   public class HPStatus {

      #region Data declarations and Properties

      private HitachiPrinter HP;

      private string[] description;
      private Color[] severity;
      private StateChange state;

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

      #endregion

      #region ServiceRoutines

      public HPStatus(HitachiPrinter hp) {
         this.HP = hp;
         description = new string[4] { "N/A", "N/A", "N/A", "N/A" };
         severity = new Color[4] { Color.Gray, Color.Gray, Color.Gray, Color.Gray };
      }

      public string GetDescription(StatusAreas whichArea) {
         return description[(int)whichArea];
      }

      public void SetDescription(StatusAreas whichArea, string description) {
         this.description[(int)whichArea] = description;
      }

      public void ResetDescriptions() {
         this.description[(int)StatusAreas.Connection] = "N/A";
         this.description[(int)StatusAreas.Reception] = "N/A";
         this.description[(int)StatusAreas.Operation] = "N/A";
         this.description[(int)StatusAreas.Alarm] = "N/A";
      }

      public Color GetSeverity(StatusAreas whichArea) {
         return severity[(int)whichArea];
      }

      public void SetSeverity(StatusAreas whichArea, Color severity) {
         this.severity[(int)whichArea] = severity;
      }

      public void SetAllSeverity(Color severity) {
         this.severity[(int)StatusAreas.Connection] = severity;
         this.severity[(int)StatusAreas.Reception] = severity;
         this.severity[(int)StatusAreas.Operation] = severity;
         this.severity[(int)StatusAreas.Alarm] = severity;
      }

      public Color GetMergedSeverity() {
         Color result = Color.LightGreen;
         if (state == StateChange.OffLine || state == StateChange.Disconnected) {
            result = Color.LightGray;
         } else {
            if (HP.nNAKs > 0) {
               result = Color.Pink;
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
            string nACK = $"ACKs {HP.nACKs}";
            string nNAK = $"NAKs {HP.nNAKs}";
            string state = GetState();
            statusLine = $"{state} / {connection} / {reception} / {operation} / {alarm} / {nACK} / {nNAK}";
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

      public string TranslateStatus(StatusAreas Area, char Value) {
         if (Stats == null) {
            BuildStatuscodes();
         }
         string Result;
         int i = Stats.FindIndex(x => x.Area == Area && x.Value == Value);
         if (i >= 0) {
            Result = Stats[i].Status;
         } else {
            Result = $"Unknown({Area}/0x{(int)Value}:X2)";
         }
         return Result;
      }

      #endregion

      #region Status Simulation

      private char[] status = new char[] { HitachiPrinter.cSTX, '1', '0', '0', '2', '0', HitachiPrinter .cETX };

      public char Connection { set { status[2] = value; } }
      public char Reception { set { status[3] = value; } }
      public char Operation { set { status[4] = value; } }
      public char Alarm { set { status[5] = value; } }

      public string Status { get { return new string(status); } set { status = value.ToCharArray(); } }

      #endregion

      #region Translation Tables

      class Stat {
         public Stat(StatusAreas Area, char Value, string Status) {
            this.Area = Area;
            this.Value = Value;
            this.Status = Status;
         }
         public StatusAreas Area;
         public char Value;
         public string Status;
      }

      static List<Stat> Stats = null;


      void BuildStatuscodes() {
         Stats = new List<Stat> {
            new Stat(StatusAreas.Connection, '\x30', "Offline"),
            new Stat(StatusAreas.Connection, '\x31', "Online"),

            new Stat(StatusAreas.Reception, '\x30', "Reception not possible"),
            new Stat(StatusAreas.Reception, '\x31', "Reception possible"),

            new Stat(StatusAreas.Operation, '\x30', "Paused"),
            new Stat(StatusAreas.Operation, '\x31', "Running - Not Ready"),
            new Stat(StatusAreas.Operation, '\x32', "Ready"),
            new Stat(StatusAreas.Operation, '\x33', "Deflection Voltage Fault"),
            new Stat(StatusAreas.Operation, '\x34', "Main Ink Tank Too Full"),
            new Stat(StatusAreas.Operation, '\x35', "Blank Print Items"),
            new Stat(StatusAreas.Operation, '\x36', "Ink Drop Charge Too Low"),
            new Stat(StatusAreas.Operation, '\x37', "Ink Drop Charge Too High"),
            new Stat(StatusAreas.Operation, '\x38', "Print Head Cover Open"),
            new Stat(StatusAreas.Operation, '\x39', "Target Sensor Fault"),
            new Stat(StatusAreas.Operation, '\x3a', "System Operation Error C"),
            new Stat(StatusAreas.Operation, '\x3b', "Target Spacing Too Close"),
            new Stat(StatusAreas.Operation, '\x3c', "Improper Sensor Position"),
            new Stat(StatusAreas.Operation, '\x3d', "System Operation Error M"),
            new Stat(StatusAreas.Operation, '\x3e', "Charge Voltage Fault"),
            new Stat(StatusAreas.Operation, '\x3f', "Barcode Short On Numbers"),
            new Stat(StatusAreas.Operation, '\x41', "Multi DC Power Supply Fan Fault"),
            new Stat(StatusAreas.Operation, '\x42', "Deflection Voltage Leakage"),
            new Stat(StatusAreas.Operation, '\x43', "Print Overlap Fault"),
            new Stat(StatusAreas.Operation, '\x44', "Ink Low Fault"),
            new Stat(StatusAreas.Operation, '\x45', "Makeup Ink Low Fault"),
            new Stat(StatusAreas.Operation, '\x46', "Print Data Changeover In Progress M"),
            new Stat(StatusAreas.Operation, '\x47', "Excessive Format Count"),
            new Stat(StatusAreas.Operation, '\x48', "Makeup Ink Replenishment Time-out"),
            new Stat(StatusAreas.Operation, '\x49', "Stopping"),
            new Stat(StatusAreas.Operation, '\x4a', "Ink Replenishment Time-out"),
            new Stat(StatusAreas.Operation, '\x4b', "No Ink Drop Charge"),
            new Stat(StatusAreas.Operation, '\x4c', "Ink Heating Unit Too High"),
            new Stat(StatusAreas.Operation, '\x4d', "Ink Heating Unit Temperature Sensor Fault"),
            new Stat(StatusAreas.Operation, '\x4e', "Ink Heating Unit Over Current"),
            new Stat(StatusAreas.Operation, '\x4f', "Internal Communication Error C"),
            new Stat(StatusAreas.Operation, '\x50', "Internal Communication Error M"),
            new Stat(StatusAreas.Operation, '\x51', "Internal Communication Error S"),
            new Stat(StatusAreas.Operation, '\x52', "System Operation Error S"),
            new Stat(StatusAreas.Operation, '\x53', "Memory Fault C"),
            new Stat(StatusAreas.Operation, '\x54', "Memory Fault M"),
            new Stat(StatusAreas.Operation, '\x55', "Ambient Temperature Sensor Fault"),
            new Stat(StatusAreas.Operation, '\x56', "Print Controller Cooling Fan Fault"),
            new Stat(StatusAreas.Operation, '\x59', "Print Data Changeover In Progress S"),
            new Stat(StatusAreas.Operation, '\x5a', "Print Data Changeover In Progress V"),
            new Stat(StatusAreas.Operation, '\x5c', "Maint. Running"),
            new Stat(StatusAreas.Operation, '\x5d', "Memory Fault S"),
            new Stat(StatusAreas.Operation, '\x5e', "Pump Motor Fault"),
            new Stat(StatusAreas.Operation, '\x5f', "Viscometer Ink Temperature Sensor Fault"),
            new Stat(StatusAreas.Operation, '\x60', "External Communication Error"),
            new Stat(StatusAreas.Operation, '\x61', "External Signal Error"),
            new Stat(StatusAreas.Operation, '\x62', "Memory Fault OP"),
            new Stat(StatusAreas.Operation, '\x63', "Ink Heating Unit Temperature Low"),
            new Stat(StatusAreas.Operation, '\x64', "Model-key Fault"),
            new Stat(StatusAreas.Operation, '\x65', "Language-key Fault"),
            new Stat(StatusAreas.Operation, '\x66', "Communication Buffer Fault"),
            new Stat(StatusAreas.Operation, '\x67', "Shutdown Fault"),
            new Stat(StatusAreas.Operation, '\x68', "Count Overflow"),
            new Stat(StatusAreas.Operation, '\x69', "Data changeover timing fault"),
            new Stat(StatusAreas.Operation, '\x6a', "Count changeover timing fault"),
            new Stat(StatusAreas.Operation, '\x6b', "Print start timing fault"),
            new Stat(StatusAreas.Operation, '\x6c', "Ink Shelf Life Information"),
            new Stat(StatusAreas.Operation, '\x6d', "Makeup Shelf Life Information"),
            new Stat(StatusAreas.Operation, '\x71', "Print Data Changeover Error C"),
            new Stat(StatusAreas.Operation, '\x72', "Print Data Changeover Error M"),

            new Stat(StatusAreas.Alarm, '\x30', "No Alarm"),
            new Stat(StatusAreas.Alarm, '\x31', "Ink Low Warning"),
            new Stat(StatusAreas.Alarm, '\x32', "Makeup ink Low Warning"),
            new Stat(StatusAreas.Alarm, '\x33', "Ink Shelf Life Exceeded"),
            new Stat(StatusAreas.Alarm, '\x34', "Battery Low M"),
            new Stat(StatusAreas.Alarm, '\x35', "Ink Pressure High"),
            new Stat(StatusAreas.Alarm, '\x36', "Product Speed Matching Error"),
            new Stat(StatusAreas.Alarm, '\x37', "External Communication Error nnn"),
            new Stat(StatusAreas.Alarm, '\x38', "Ambient Temperature Too High"),
            new Stat(StatusAreas.Alarm, '\x39', "Ambient Temperature Too Low"),
            new Stat(StatusAreas.Alarm, '\x3a', "Ink heating failure"),
            new Stat(StatusAreas.Alarm, '\x3b', "External Signal Error nnn"),
            new Stat(StatusAreas.Alarm, '\x3c', "Ink Pressure Low"),
            new Stat(StatusAreas.Alarm, '\x3d', "Excitation V-ref. Review"),
            new Stat(StatusAreas.Alarm, '\x3e', "Viscosity Reading Instability"),
            new Stat(StatusAreas.Alarm, '\x3f', "Viscosity Readings Out of Range"),
            new Stat(StatusAreas.Alarm, '\x40', "High Ink Viscosity"),
            new Stat(StatusAreas.Alarm, '\x41', "Low Ink Viscosity"),
            new Stat(StatusAreas.Alarm, '\x42', "Excitation V-ref. Review 2"),
            new Stat(StatusAreas.Alarm, '\x44', "Battery Low C"),
            new Stat(StatusAreas.Alarm, '\x45', "Calendar Content Inaccurate"),
            new Stat(StatusAreas.Alarm, '\x46', "Excitation V-ref. Char. height Review"),
            new Stat(StatusAreas.Alarm, '\x47', "Ink Shelf Life Information"),
            new Stat(StatusAreas.Alarm, '\x48', "Makeup Shelf Life Information"),
            new Stat(StatusAreas.Alarm, '\x49', "Model-key Failure"),
            new Stat(StatusAreas.Alarm, '\x4a', "Language-key Failure"),
            new Stat(StatusAreas.Alarm, '\x4c', "Upgrade-Key Fault"),
            new Stat(StatusAreas.Alarm, '\x50', "Circulation System Cooling Fan Fault"),
            new Stat(StatusAreas.Alarm, '\x51', "Ink Tempurature Too High"),
         };
      }

      #endregion

   }

}