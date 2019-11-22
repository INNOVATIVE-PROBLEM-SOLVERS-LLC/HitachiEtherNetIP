using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IJPLibXML {
   public class Status {

      public enum Area {
         Connection = 0,
         Reception = 1,
         Operation = 2,
         Alarm = 3
      }

      public string TranslateStatus(Status.Area Area, char Value) {
         if (Stats == null) {
            BuildStatuscodes();
         }
         string Result;
         int i = Stats.FindIndex(x => x.Area == Area && x.Value == Value);
         if (i >= 0) {
            Result = Stats[i].Status;
         } else {
            Result = $"Unknown({Area}/0x{(int)Value:X2})";
         }
         return Result;
      }

      private static List<Stat> Stats = null;

      private class Stat {
         public Stat(Status.Area Area, char Value, string Status) {
            this.Area = Area;
            this.Value = Value;
            this.Status = Status;
         }
         public Status.Area Area;
         public char Value;
         public string Status;
      }

      private void BuildStatuscodes() {
         Stats = new List<Stat> {
            new Stat(Status.Area.Connection, '\x30', "Offline"),
            new Stat(Status.Area.Connection, '\x31', "Online"),

            new Stat(Status.Area.Reception, '\x30', "Reception not possible"),
            new Stat(Status.Area.Reception, '\x31', "Reception possible"),

            new Stat(Status.Area.Operation, '\x30', "Paused"),
            new Stat(Status.Area.Operation, '\x31', "Running - Not Ready"),
            new Stat(Status.Area.Operation, '\x32', "Ready"),
            new Stat(Status.Area.Operation, '\x33', "Deflection Voltage Fault"),
            new Stat(Status.Area.Operation, '\x34', "Main Ink Tank Too Full"),
            new Stat(Status.Area.Operation, '\x35', "Blank Print Items"),
            new Stat(Status.Area.Operation, '\x36', "Ink Drop Charge Too Low"),
            new Stat(Status.Area.Operation, '\x37', "Ink Drop Charge Too High"),
            new Stat(Status.Area.Operation, '\x38', "Print Head Cover Open"),
            new Stat(Status.Area.Operation, '\x39', "Target Sensor Fault"),
            new Stat(Status.Area.Operation, '\x3a', "System Operation Error C"),
            new Stat(Status.Area.Operation, '\x3b', "Target Spacing Too Close"),
            new Stat(Status.Area.Operation, '\x3c', "Improper Sensor Position"),
            new Stat(Status.Area.Operation, '\x3d', "System Operation Error M"),
            new Stat(Status.Area.Operation, '\x3e', "Charge Voltage Fault"),
            new Stat(Status.Area.Operation, '\x3f', "Barcode Short On Numbers"),
            new Stat(Status.Area.Operation, '\x41', "Multi DC Power Supply Fan Fault"),
            new Stat(Status.Area.Operation, '\x42', "Deflection Voltage Leakage"),
            new Stat(Status.Area.Operation, '\x43', "Print Overlap Fault"),
            new Stat(Status.Area.Operation, '\x44', "Ink Low Fault"),
            new Stat(Status.Area.Operation, '\x45', "Makeup Ink Low Fault"),
            new Stat(Status.Area.Operation, '\x46', "Print Data Changeover In Progress M"),
            new Stat(Status.Area.Operation, '\x47', "Excessive Format Count"),
            new Stat(Status.Area.Operation, '\x48', "Makeup Ink Replenishment Time-out"),
            new Stat(Status.Area.Operation, '\x49', "Stopping"),
            new Stat(Status.Area.Operation, '\x4a', "Ink Replenishment Time-out"),
            new Stat(Status.Area.Operation, '\x4b', "No Ink Drop Charge"),
            new Stat(Status.Area.Operation, '\x4c', "Ink Heating Unit Too High"),
            new Stat(Status.Area.Operation, '\x4d', "Ink Heating Unit Temperature Sensor Fault"),
            new Stat(Status.Area.Operation, '\x4e', "Ink Heating Unit Over Current"),
            new Stat(Status.Area.Operation, '\x4f', "Internal Communication Error C"),
            new Stat(Status.Area.Operation, '\x50', "Internal Communication Error M"),
            new Stat(Status.Area.Operation, '\x51', "Internal Communication Error S"),
            new Stat(Status.Area.Operation, '\x52', "System Operation Error S"),
            new Stat(Status.Area.Operation, '\x53', "Memory Fault C"),
            new Stat(Status.Area.Operation, '\x54', "Memory Fault M"),
            new Stat(Status.Area.Operation, '\x55', "Ambient Temperature Sensor Fault"),
            new Stat(Status.Area.Operation, '\x56', "Print Controller Cooling Fan Fault"),
            new Stat(Status.Area.Operation, '\x59', "Print Data Changeover In Progress S"),
            new Stat(Status.Area.Operation, '\x5a', "Print Data Changeover In Progress V"),
            new Stat(Status.Area.Operation, '\x5c', "Maint. Running"),
            new Stat(Status.Area.Operation, '\x5d', "Memory Fault S"),
            new Stat(Status.Area.Operation, '\x5e', "Pump Motor Fault"),
            new Stat(Status.Area.Operation, '\x5f', "Viscometer Ink Temperature Sensor Fault"),
            new Stat(Status.Area.Operation, '\x60', "External Communication Error"),
            new Stat(Status.Area.Operation, '\x61', "External Signal Error"),
            new Stat(Status.Area.Operation, '\x62', "Memory Fault OP"),
            new Stat(Status.Area.Operation, '\x63', "Ink Heating Unit Temperature Low"),
            new Stat(Status.Area.Operation, '\x64', "Model-key Fault"),
            new Stat(Status.Area.Operation, '\x65', "Language-key Fault"),
            new Stat(Status.Area.Operation, '\x66', "Communication Buffer Fault"),
            new Stat(Status.Area.Operation, '\x67', "Shutdown Fault"),
            new Stat(Status.Area.Operation, '\x68', "Count Overflow"),
            new Stat(Status.Area.Operation, '\x69', "Data changeover timing fault"),
            new Stat(Status.Area.Operation, '\x6a', "Count changeover timing fault"),
            new Stat(Status.Area.Operation, '\x6b', "Print start timing fault"),
            new Stat(Status.Area.Operation, '\x6c', "Ink Shelf Life Information"),
            new Stat(Status.Area.Operation, '\x6d', "Makeup Shelf Life Information"),
            new Stat(Status.Area.Operation, '\x71', "Print Data Changeover Error C"),
            new Stat(Status.Area.Operation, '\x72', "Print Data Changeover Error M"),

            new Stat(Status.Area.Alarm, '\x30', "No Alarm"),
            new Stat(Status.Area.Alarm, '\x31', "Ink Low Warning"),
            new Stat(Status.Area.Alarm, '\x32', "Makeup ink Low Warning"),
            new Stat(Status.Area.Alarm, '\x33', "Ink Shelf Life Exceeded"),
            new Stat(Status.Area.Alarm, '\x34', "Battery Low M"),
            new Stat(Status.Area.Alarm, '\x35', "Ink Pressure High"),
            new Stat(Status.Area.Alarm, '\x36', "Product Speed Matching Error"),
            new Stat(Status.Area.Alarm, '\x37', "External Communication Error nnn"),
            new Stat(Status.Area.Alarm, '\x38', "Ambient Temperature Too High"),
            new Stat(Status.Area.Alarm, '\x39', "Ambient Temperature Too Low"),
            new Stat(Status.Area.Alarm, '\x3a', "Ink heating failure"),
            new Stat(Status.Area.Alarm, '\x3b', "External Signal Error nnn"),
            new Stat(Status.Area.Alarm, '\x3c', "Ink Pressure Low"),
            new Stat(Status.Area.Alarm, '\x3d', "Excitation V-ref. Review"),
            new Stat(Status.Area.Alarm, '\x3e', "Viscosity Reading Instability"),
            new Stat(Status.Area.Alarm, '\x3f', "Viscosity Readings Out of Range"),
            new Stat(Status.Area.Alarm, '\x40', "High Ink Viscosity"),
            new Stat(Status.Area.Alarm, '\x41', "Low Ink Viscosity"),
            new Stat(Status.Area.Alarm, '\x42', "Excitation V-ref. Review 2"),
            new Stat(Status.Area.Alarm, '\x44', "Battery Low C"),
            new Stat(Status.Area.Alarm, '\x45', "Calendar Content Inaccurate"),
            new Stat(Status.Area.Alarm, '\x46', "Excitation V-ref. Char. height Review"),
            new Stat(Status.Area.Alarm, '\x47', "Ink Shelf Life Information"),
            new Stat(Status.Area.Alarm, '\x48', "Makeup Shelf Life Information"),
            new Stat(Status.Area.Alarm, '\x49', "Model-key Failure"),
            new Stat(Status.Area.Alarm, '\x4a', "Language-key Failure"),
            new Stat(Status.Area.Alarm, '\x4c', "Upgrade-Key Fault"),
            new Stat(Status.Area.Alarm, '\x50', "Circulation System Cooling Fan Fault"),
            new Stat(Status.Area.Alarm, '\x51', "Ink Tempurature Too High"),
         };
      }

   }

}
