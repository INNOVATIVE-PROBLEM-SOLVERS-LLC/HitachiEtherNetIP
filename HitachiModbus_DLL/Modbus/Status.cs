using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus_DLL {
   static public class Status {

      // Status areas
      public enum StatusAreas {
         Connection = 0,
         Reception = 1,
         Operation = 2,
         Warning = 3,
         Analysis1 = 4,
         Analysis2 = 5,
         Analysis3 = 6,
         Analysis4 = 7,
      }

      static public string TranslateStatus(StatusAreas Area, int Value) {
         string Result;
         int i = Stats.FindIndex(x => x.Area == Area && x.Value == Value);
         if (i >= 0) {
            Result = Stats[i].Status;
         } else {
            Result = $"Unknown({Area}/0x{(int)Value}:X2)";
         }
         return Result;
      }

      public static List<Stat> Stats = new List<Stat> {
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

         new Stat(StatusAreas.Warning, '\x30', "No Alarm"),
         new Stat(StatusAreas.Warning, '\x31', "Ink Low Warning"),
         new Stat(StatusAreas.Warning, '\x32', "Makeup ink Low Warning"),
         new Stat(StatusAreas.Warning, '\x33', "Ink Shelf Life Exceeded"),
         new Stat(StatusAreas.Warning, '\x34', "Battery Low M"),
         new Stat(StatusAreas.Warning, '\x35', "Ink Pressure High"),
         new Stat(StatusAreas.Warning, '\x36', "Product Speed Matching Error"),
         new Stat(StatusAreas.Warning, '\x37', "External Communication Error nnn"),
         new Stat(StatusAreas.Warning, '\x38', "Ambient Temperature Too High"),
         new Stat(StatusAreas.Warning, '\x39', "Ambient Temperature Too Low"),
         new Stat(StatusAreas.Warning, '\x3a', "Ink heating failure"),
         new Stat(StatusAreas.Warning, '\x3b', "External Signal Error nnn"),
         new Stat(StatusAreas.Warning, '\x3c', "Ink Pressure Low"),
         new Stat(StatusAreas.Warning, '\x3d', "Excitation V-ref. Review"),
         new Stat(StatusAreas.Warning, '\x3e', "Viscosity Reading Instability"),
         new Stat(StatusAreas.Warning, '\x3f', "Viscosity Readings Out of Range"),
         new Stat(StatusAreas.Warning, '\x40', "High Ink Viscosity"),
         new Stat(StatusAreas.Warning, '\x41', "Low Ink Viscosity"),
         new Stat(StatusAreas.Warning, '\x42', "Excitation V-ref. Review 2"),
         new Stat(StatusAreas.Warning, '\x44', "Battery Low C"),
         new Stat(StatusAreas.Warning, '\x45', "Calendar Content Inaccurate"),
         new Stat(StatusAreas.Warning, '\x46', "Excitation V-ref. Char. height Review"),
         new Stat(StatusAreas.Warning, '\x47', "Ink Shelf Life Information"),
         new Stat(StatusAreas.Warning, '\x48', "Makeup Shelf Life Information"),
         new Stat(StatusAreas.Warning, '\x49', "Model-key Failure"),
         new Stat(StatusAreas.Warning, '\x4a', "Language-key Failure"),
         new Stat(StatusAreas.Warning, '\x4c', "Upgrade-Key Fault"),
         new Stat(StatusAreas.Warning, '\x50', "Circulation System Cooling Fan Fault"),
         new Stat(StatusAreas.Warning, '\x51', "Ink Tempurature Too High"),

         new Stat(StatusAreas.Analysis1, '\x10', "Write on Holding Register"),
         new Stat(StatusAreas.Analysis1, '\x06', "Write one word on Holding Register"),
         new Stat(StatusAreas.Analysis1, '\x03', "Read Holding Register"),
         new Stat(StatusAreas.Analysis1, '\x04', "Read Input Register"),
         new Stat(StatusAreas.Analysis1, '\x00', "None"),

         new Stat(StatusAreas.Analysis2, '\x00', "None"),
         new Stat(StatusAreas.Analysis2, '\x01', "Index"),
         new Stat(StatusAreas.Analysis2, '\x02', "Print description"),
         new Stat(StatusAreas.Analysis2, '\x03', "Delete of Print contents"),
         new Stat(StatusAreas.Analysis2, '\x04', "Print data Recall"),
         new Stat(StatusAreas.Analysis2, '\x05', "Print data Register"),
         new Stat(StatusAreas.Analysis2, '\x06', "Print format"),
         new Stat(StatusAreas.Analysis2, '\x07', "Print specification"),
         new Stat(StatusAreas.Analysis2, '\x08', "Calendar conditions"),
         new Stat(StatusAreas.Analysis2, '\x09', "Substitution rules"),
         new Stat(StatusAreas.Analysis2, '\x0A', "Time count"),
         new Stat(StatusAreas.Analysis2, '\x0B', "Shift code"),
         new Stat(StatusAreas.Analysis2, '\x0C', "Count conditions"),
         new Stat(StatusAreas.Analysis2, '\x0D', "Various print setup"),
         new Stat(StatusAreas.Analysis2, '\x0E', "Online/Offline"),
         new Stat(StatusAreas.Analysis2, '\x0F', "Remote operation"),
         new Stat(StatusAreas.Analysis2, '\x10', "Time control"),
         new Stat(StatusAreas.Analysis2, '\x11', "User environment setup (Preparation for future)"),
         new Stat(StatusAreas.Analysis2, '\x12', "Touch screen setup"),
         new Stat(StatusAreas.Analysis2, '\x14', "Operation management"),
         new Stat(StatusAreas.Analysis2, '\x15', "Circulation control"),
         new Stat(StatusAreas.Analysis2, '\x16', "Print data management"),
         new Stat(StatusAreas.Analysis2, '\x17', "Group control"),
         new Stat(StatusAreas.Analysis2, '\x18', "Editing Substitution rules"),
         new Stat(StatusAreas.Analysis2, '\x1A', "Print job control (Preparation for future)"),
         new Stat(StatusAreas.Analysis2, '\x1B', "User pattern (Fixed size)"),
         new Stat(StatusAreas.Analysis2, '\x0C', "User pattern (Free size)"),

         new Stat(StatusAreas.Analysis3, '\x00', "None"),
         new Stat(StatusAreas.Analysis3, '\x01', "Invalid Function code"),
         new Stat(StatusAreas.Analysis3, '\x02', "Invalid Address"),
         new Stat(StatusAreas.Analysis3, '\x03', "Invalid Data size"),
         new Stat(StatusAreas.Analysis3, '\x04', "Invalid CRC code"),
         new Stat(StatusAreas.Analysis3, '\x05', "Invalid Offline"),
         new Stat(StatusAreas.Analysis3, '\x06', "Invalid Message repeat print"),
         new Stat(StatusAreas.Analysis3, '\x07', "Communication is NOT allowed when Ink ejection is stopped"),
         new Stat(StatusAreas.Analysis3, '\x10', "Invalid data"),

         new Stat(StatusAreas.Analysis4, '\x00', "Fixed"),
      };

   }

   public class Stat {
      public Stat(Status.StatusAreas Area, char Value, string Status) {
         this.Area = Area;
         this.Value = Value;
         this.Status = Status;
      }
      public Status.StatusAreas Area;
      public char Value;
      public string Status;
   }


}
