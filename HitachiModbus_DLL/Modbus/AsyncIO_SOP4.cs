using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus_DLL;



namespace Modbus_DLL {

   #region AsyncIO for SOP-04

   public partial class AsyncIO {

      // Same as enuComCharacters except as strings
      const string sNUL = "\x00";     // 00h Null Character
      const string sSTX = "\x02";     // 02h Start of Text
      const string sETX = "\x03";     // 03h End of Text
      const string sENQ = "\x05";     // 05h Enquire
      const string sACK = "\x06";     // 06h Acknowledge
      const string sBEL = "\x07";     // 07h Bell
      const string sLF = "\x0A";      // 0Ah Line Feed
      const string sCR = "\x0D";      // 0Dh Carriage Return
      const string sSO = "\x0E";      // 0Eh Shift Out
      const string scSI = "\x0F";     // 0Fh Shift In
      const string sDLE = "\x10";     // 10h Data Link Escape
      const string sDC2 = "\x12";     // 12h Device Control 2
      const string sDC3 = "\x13";     // 13h Device Control 3
      const string sNAK = "\x15";     // 15h Negative Acknowledge
      const string sEM = "\x19";      // 19h End of Medium
      const string sESC = "\x1B";     // 1Bh Escape for PX and PXR printers
      const string sESC2 = "\x1F";    // 1Fh Escape for RX printer
      const string sSpace = "\x20";   // 20h Space character
      const string sTilde = "\x7E";   // 7Eh Tilde character

      public enum RetrieveOps {
         LineSetting = 0,               // 00 PXR C0 C1 RX C0 31 == Complete
         PrintContentsAttributes,       // 01 PXR C0 C2 RX C0 32 == Needed
         PrintContentsNoAttributes,     // 02 PXR C0 C3 RX C0 33 == Needed
         CalendarCondition,             // 03 PXR C0 C4 RX C0 34 == Complete
         SubstitutionRule,              // 04 PXR C0 C5 RX C0 35
         SubstitutionRuleData,          // 05 PXR       RX C0 36
         ShiftCodeSetup,                // 06 PXR C0 D5 RX C0 37 == Complete
         TimeCountCondition,            // 07 PXR C0 D6 RX C0 38 == Complete
         CountCondition,                // 08 PXR C0 C6 RX C0 39 == Complete
         PrintFormat,                   // 09 PXR C0 C7 RX C0 3A == Complete
         AdjustICS,                     // 10 PXR       RX C0 3B
         PrintSpecifications,           // 11 PXR C0 C8 RX C0 3C
         VariousPrintSetup,             // 12 PXR       RX C0 3D
         MessageGroupNames,             // 13 PXR       RX CO 3E == Complete
         PrintData,                     // 14 PXR C0 C9 RX C0 3F == Complete
         UserEnvironmentSetup,          // 15 PXR C0 CA RX C0 40 == Complete
         DateTimeSetup,                 // 16 PXR C0 CB RX C0 41 == Complete
         CommunicationsSetup,           // 17 PXR C0 CC RX C0 42
         TouchScreenSetup,              // 18 PXR C0 CD RX C0 43
         UnitInformation,               // 19 PXR D0 D1 RX C0 47
         OperationManagement,           // 20 PXR C0 CE RX C0 48
         AlarmHistory,                  // 21 PXR C0 CF RX C0 49 == Complete
         PartsUsageTime,                // 22 PXR C0 D1 RX C0 4A
         CirculationSystemSetup,        // 23 PXR       RX C0 4B
         SoftwareVersion,               // 24 PXR C0 D2 RX C0 4C == Complete
         AdjustmentOperationalCheckout, // 25 PXR D0 D4 RX C0 4D
         SolenoidValvePumpTest,         // 26 PXR DO D5 RX C0 4E
         FreeLayoutCoordinates,         // 27 PXR       RX C0 50 == Complete
         StirrerTest,                   // 28 PXR C0 D3
         MonthSubstituteRule,           // 29 PXR C0 D4
         ViscometerCalibration,         // 30 PXR D0 D2
         SystemEnvironmentSetup,        // 31 PXR D0 D3
         UserPatternFixed,
         UserPatternFree,
         StandardCharacterPattern,
      }


      internal void Retrieve(ModbusPkt pkt) {
         string response = "\x02\x03";
         MB.Errors = 0;
         try {
            switch ((RetrieveOps)pkt.SubOp) {
               case RetrieveOps.LineSetting:
                  response = LineSetting();
                  break;
               case RetrieveOps.PrintContentsAttributes:
                  response = PrintContentsAttributes();
                  break;
               case RetrieveOps.PrintContentsNoAttributes:
                  response = PrintContentsNoAttributes();
                  break;
               case RetrieveOps.CalendarCondition:
                  response = CalendarCondition();
                  break;
               case RetrieveOps.SubstitutionRule:
                  response = SubstitutionRule();
                  break;
               case RetrieveOps.SubstitutionRuleData:
                  response = SubstitutionRuleData();
                  break;
               case RetrieveOps.ShiftCodeSetup:
                  response = ShiftCodeSetup();
                  break;
               case RetrieveOps.TimeCountCondition:
                  response = TimeCountCondition();
                  break;
               case RetrieveOps.CountCondition:
                  response = CountCondition();
                  break;
               case RetrieveOps.PrintFormat:
                  response = PrintFormat();
                  break;
               case RetrieveOps.AdjustICS:
                  response = AdjustICS();
                  break;
               case RetrieveOps.PrintSpecifications:
                  response = PrintSpecifications();
                  break;
               case RetrieveOps.VariousPrintSetup:
                  response = VariousPrintSetup();
                  break;
               case RetrieveOps.MessageGroupNames:
                  response = MessageGroupNames();
                  break;
               case RetrieveOps.PrintData:
                  response = PrintData();
                  break;
               case RetrieveOps.UserEnvironmentSetup:
                  response = UserEnvironmentSetup();
                  break;
               case RetrieveOps.DateTimeSetup:
                  response = DateTimeSetup();
                  break;
               case RetrieveOps.CommunicationsSetup:
                  response = CommunicationsSetup();
                  break;
               case RetrieveOps.TouchScreenSetup:
                  response = TouchScreenSetup();
                  break;
               case RetrieveOps.UnitInformation:
                  response = UnitInformation();
                  break;
               case RetrieveOps.OperationManagement:
                  response = OperationManagement();
                  break;
               case RetrieveOps.AlarmHistory:
                  response = AlarmHistory();
                  break;
               case RetrieveOps.PartsUsageTime:
                  response = PartsUsageTime();
                  break;
               case RetrieveOps.CirculationSystemSetup:
                  response = CirculationSystemSetup();
                  break;
               case RetrieveOps.SoftwareVersion:
                  response = SoftwareVersion();
                  break;
               case RetrieveOps.AdjustmentOperationalCheckout:
                  response = AdjustmentOperationalCheckout();
                  break;
               case RetrieveOps.SolenoidValvePumpTest:
                  response = SolenoidValvePumpTest();
                  break;
               case RetrieveOps.FreeLayoutCoordinates:
                  response = FreeLayoutCoordinates();
                  break;
               case RetrieveOps.StirrerTest:
                  response = StirrerTest();
                  break;
               case RetrieveOps.MonthSubstituteRule:
                  response = MonthSubstituteRule();
                  break;
               case RetrieveOps.ViscometerCalibration:
                  response = ViscometerCalibration();
                  break;
               case RetrieveOps.SystemEnvironmentSetup:
                  response = SystemEnvironmentSetup();
                  break;
               case RetrieveOps.UserPatternFixed:
                  response = UserPatternFixed(pkt);
                  break;
               case RetrieveOps.StandardCharacterPattern:
                  response = StandardCharacterPattern(pkt);
                  break;
               case RetrieveOps.UserPatternFree:
                  response = UserPatternFree(pkt);
                  break;
               default:
                  break;
            }
         } catch (Exception e) {
            Log(this, $"Retrieve \"{(RetrieveOps)pkt.SubOp}\" failed! -- {e.Message}");
            MB.Errors = 1;
         }
         AsyncComplete ac = new AsyncComplete(MB, pkt) { Resp1 = response, Success = MB.Errors == 0 };
         parent.Invoke(new EventHandler(delegate { Complete(this, ac); }));
      }

      // Complete
      private string LineSetting() {
         int[] cpi = GetTextLengths(out int itemCount, out int charCount);      //

         int layout = MB.GetDecAttribute(ccPF.Format_Setup);                    // Message Layout
         int n = 0;
         List<int> rows = new List<int>();                                      // Holds the number of rows in each column
         List<int> spacing = new List<int>();                                   // Holds the line spacing
         List<int> dups = new List<int>();                                      // Adjacent duplicate columns
         int lc;                                                                // Line count
         int ls;                                                                // Line spacing
         while (n < itemCount) {                                                // Read all columns
            Section<ccPF> pf = new Section<ccPF>(MB, ccPF.Line_Count, n, 24);
            lc = pf.GetDec(ccPF.Line_Count);                                    // Number of lines for this column
            ls = pf.GetDec(ccPF.Line_Spacing);                                  // Spacing between lines
            if (cpi[n] == 0) {
               rows.Add(lc);                                                    // Create a single entry for all remaining
               spacing.Add(ls);
               dups.Add((itemCount - n) / lc);
               n = itemCount;
            } else {
               if (rows.Count > 0                                               // Is this a dup of previous column
                  && rows[rows.Count - 1] == lc
                  && spacing[spacing.Count - 1] == ls) {
                  dups[dups.Count - 1]++;                                       // Just up the count
               } else {
                  rows.Add(lc);                                                 // Create a new entry
                  spacing.Add(ls);
                  dups.Add(1);
               }
               n += lc;                                                         // Move on to the next column
            }
         }

         StringBuilder sb = new StringBuilder(sSTX, 2000);
         sb.Append(sESC2);                                                      // Send Header
         sb.AppendFormat("{0:D1}", layout);
         for (int i = 0; i < rows.Count; i++) {
            sb.Append(sESC2);                                                   // Send column info
            sb.Append(((char)(dups[i] + '0')).ToString());                      // 0x31 to 0x94
            sb.Append(((char)(rows[i] + '0')).ToString());
         }
         return sb.Append(sETX).ToString();
      }

      // Complete
      private string PrintContentsAttributes() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         byte[][] text = GetMessageTextAsBytes();                             // Get text for each item
         for (int i = 0; i < text.Length; i++) {
            if (text[i] != null) {                                     // Ignore empty items
               sb.Append(sDLE + (char)('1' + i));                      // Item Header
               for (int j = 0; j < text[i].Length; j += 4) {           // Text saved as 4 bytes
                  byte b0 = text[i][j];                                // Get shortcut for indicator byte
                  byte b1 = text[i][j + 1];
                  byte b2 = text[i][j + 2];
                  byte b3 = text[i][j + 3];
                  if (text[i][j] == 0) {                               // Leading byte 0 implies UTF8 Character
                     sb.Append(((char)b0).ToString());                 // Output null character
                     sb.Append(((char)b2).ToString());                 // Output UTF8 character (2 bytes)
                     sb.Append(((char)b3).ToString());
                  } else {
                     switch (b0) {
                        case 0xF1:
                           sb.Append('\xC2'.ToString());
                           sb.Append(((char)text[i][j + 0]).ToString());
                           sb.Append(((char)text[i][j + 1]).ToString());
                           break;
                        case 0xF2:
                           if (b1 == 0x5A || b1 == 0x6A || b1 == 0x7A) {

                           } else if (b1 >= 0x50 && b1 < 0x80) {
                              sb.Append('\xC1'.ToString());
                              sb.Append(((char)text[i][j + 0]).ToString());
                              sb.Append(((char)text[i][j + 1]).ToString());
                           }
                           if (b1 > 0x20 && b1 <= 0x27) {

                           }
                           break;
                     }
                  }
               }
            }
         }
         return sb.Append(sETX).ToString();
      }

      // Complete
      private string PrintContentsNoAttributes() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);
         byte[][] text = GetMessageTextAsBytes();                            // All text by Iten Number
         for (int i = 0; i < text.Length; i++) {
            if (text[i] != null) {                                    // Null implies that it is a unused item
               sb.Append(sDLE + (char)('1' + i));
               for (int j = 0; j < text[i].Length; j += 4) {          // Attributed text is stored as 4-byte items
                  sb.Append(((char)text[i][j + 2]).ToString());       // bytes 3 and 4 are the UTF8 character
                  sb.Append(((char)text[i][j + 3]).ToString());
               }
            }
         }
         return sb.Append(sETX).ToString();
      }

      // Complete
      private string CalendarCondition() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);
         int[] cpi = GetTextLengths(out int itemCount, out int totalChars);
         for (int i = 0; i < itemCount; i++) {
            if (cpi[i] > 0) {
               int calCount = MB.GetDecAttribute(ccPF.Number_of_Calendar_Blocks, i);
               if (calCount > 0) {
                  int n = MB.GetDecAttribute(ccPF.First_Calendar_Block, i);
                  for (int j = 0; j < calCount; j++) {
                     Section<ccCal> cs = new Section<ccCal>(MB, ccCal.Offset_Year, n + j - 1, 32);
                     string[] cc = new string[4];
                     sb.Append(sDLE + (char)('1' + i));

                     sb.Append(sESC2);
                     sb.Append(cs.Get(ccCal.Offset_Year, 2));
                     sb.Append(cs.Get(ccCal.Offset_Month, 2));
                     sb.Append(cs.Get(ccCal.Offset_Day, 4));
                     sb.Append(cs.Get(ccCal.Offset_Hour, 3));
                     sb.Append(cs.Get(ccCal.Offset_Minute, 3));

                     sb.Append(sESC2);
                     sb.Append(cs.Get(ccCal.Substitute_Year, 1));
                     sb.Append(cs.Get(ccCal.Substitute_Month, 1));
                     sb.Append(cs.Get(ccCal.Substitute_Day, 1));
                     sb.Append(cs.Get(ccCal.Substitute_Hour, 1));
                     sb.Append(cs.Get(ccCal.Substitute_Minute, 1));
                     sb.Append(cs.Get(ccCal.Substitute_Weeks, 1));
                     sb.Append(cs.Get(ccCal.Substitute_DayOfWeek, 1));

                     sb.Append(sESC2);
                     sb.Append(cs.Get(ccCal.Zero_Suppress_Year, 1));
                     sb.Append(cs.Get(ccCal.Zero_Suppress_Month, 1));
                     sb.Append(cs.Get(ccCal.Zero_Suppress_Day, 1));
                     sb.Append(cs.Get(ccCal.Zero_Suppress_Hour, 1));
                     sb.Append(cs.Get(ccCal.Zero_Suppress_Minute, 1));
                     sb.Append(cs.Get(ccCal.Zero_Suppress_Weeks, 1));
                     sb.Append(cs.Get(ccCal.Zero_Suppress_DayOfWeek, 1));

                     sb.Append(sESC2);
                     sb.Append("0");                // We do not have SOP-05

                     sb.Append(sESC2);
                     sb.Append(cs.Get(ccCal.Substitute_Rule, 2));
                  }
               }
            }
         }

         return sb.Append(sETX).ToString();
      }

      private string SubstitutionRule() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         Section<ccSR> sr = new Section<ccSR>(MB, ccSR.Start_Year, 0, 0);

         sb.Append(sESC2);
         sb.Append(sr.Get(ccSR.Start_Year, 2));
         sb.Append(sr.Get(ccSR.Year, 2));
         sb.Append(sr.Get(ccSR.Month, 2));
         sb.Append(sr.Get(ccSR.Day, 2));
         sb.Append(sr.Get(ccSR.Hour, 2));
         sb.Append(sr.Get(ccSR.Minute, 2));
         sb.Append(sr.Get(ccSR.Week, 2));
         sb.Append(sr.Get(ccSR.DayOfWeek, 2));

         return sb.Append(sETX).ToString();
      }

      private string SubstitutionRuleData() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         //Section<ccSR> pf = new Section<ccSR>(MB, ccPF.Line_Count, 0, 0);

         return sb.Append(sETX).ToString();
      }

      // Complete (Work needed in cijConnect SOP routine)
      private string ShiftCodeSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         if (MB.GetAttribute(Modbus.FunctionCode.ReadInput, 1, 0x0EFD, 2, out byte[] nsc)) {
            int shiftCount = (nsc[0] << 8) + nsc[1];

            Section<ccSR> sc = new Section<ccSR>(MB, ccSR.Shift_Start_Hour, 0, 16 * shiftCount);

            for (int i = 0; i < shiftCount; i++) {
               sc.SetOffset(i);
               sb.Append(sESC2);
               sb.Append(sc.Get(ccSR.Shift_Start_Hour, 2));
               sb.Append(sc.Get(ccSR.Shift_Start_Minute, 2));
               sb.Append(sc.Get(ccSR.Shift_End_Hour, 2));
               sb.Append(sc.Get(ccSR.Shift_End_Minute, 2));
               sb.Append(sc.Get(ccSR.Shift_String_Value));
            }

            return sb.Append(sETX).ToString();
         } else {
            return "\x02\x03";
         }
      }

      // Complete
      private string TimeCountCondition() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         int x = MB.GetDecAttribute(ccUI.Shift_Code_And_Time_Count);

         Section<ccSR> tc = new Section<ccSR>(MB, ccSR.Time_Count_Start_Value, 0, 11);

         sb.Append(sESC2 + tc.Get(ccSR.Time_Count_Start_Value, 3));
         sb.Append(sESC2 + tc.Get(ccSR.Time_Count_End_Value, 3));
         sb.Append(sESC2 + tc.Get(ccSR.Time_Count_Reset_Value, 3));
         sb.Append(sESC2 + tc.Get(ccSR.Reset_Time_Value, 2));
         sb.Append(sESC2 + tc.Get(ccSR.Update_Interval_Value, 1));
         return sb.Append(sETX).ToString();
      }

      // Complete
      private string CountCondition() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         int[] cpi = GetTextLengths(out int itemCount, out int totalChars);
         for (int i = 0; i < itemCount; i++) {
            if (cpi[i] > 0) {
               int cntCount = MB.GetDecAttribute(ccPF.Number_Of_Count_Blocks, i);
               if (cntCount > 0) {
                  int n = MB.GetDecAttribute(ccPF.First_Count_Block, i);
                  for (int j = 0; j < cntCount; j++) {
                     Section<ccCount> cb = new Section<ccCount>(MB, ccCount.Initial_Value, n + j - 1, 148);

                     sb.Append(sDLE + (char)('1' + i));
                     sb.Append(sESC2 + cb.Get(ccCount.Initial_Value));
                     sb.Append(sESC2 + cb.Get(ccCount.Count_Range_1));
                     sb.Append(sESC2 + cb.Get(ccCount.Count_Range_2));
                     sb.Append(sESC2 + cb.Get(ccCount.Update_Unit_Halfway, 6));
                     sb.Append(sESC2 + cb.Get(ccCount.Update_Unit_Unit, 6));
                     sb.Append(sESC2 + cb.Get(ccCount.Increment_Value, 2));
                     sb.Append(sESC2 + cb.Get(ccCount.Direction_Value, 1));
                     sb.Append(sESC2 + cb.Get(ccCount.Jump_From));
                     sb.Append(sESC2 + cb.Get(ccCount.Jump_To));
                     sb.Append(sESC2 + cb.Get(ccCount.Reset_Value));
                     sb.Append(sESC2 + cb.Get(ccCount.External_Count, 1));
                  }
               }
            }
         }
         return sb.Append(sETX).ToString();
      }

      // Complete
      private string PrintFormat() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         int[] cpi = GetTextLengths(out int itemCount, out int totalChars);
         for (int i = 0; i < itemCount; i++) {
            Section<ccPF> pf = new Section<ccPF>(MB, ccPF.Line_Count, i, 24);
            sb.Append(sDLE + (char)('1' + i));                                  // Item Header
            sb.Append(sESC2 + pf.Get(ccPF.Line_Spacing, 1));
            sb.Append(sESC2 + pf.Get(ccPF.Dot_Matrix, 1));
            sb.Append(sESC2 + pf.Get(ccPF.InterCharacter_Space, 2));
            sb.Append(sESC2 + pf.Get(ccPF.Character_Bold, 1));
            sb.Append(sESC2 + pf.Get(ccPF.Barcode_Type, 1));
            sb.Append(sESC2 + pf.Get(ccPF.Prefix_Code, 2));
            sb.Append(sESC2 + pf.Get(ccPF.Readable_Code, 1));
         }

         return sb.Append(sETX).ToString();
      }

      private string AdjustICS() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      // Complete
      private string PrintSpecifications() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         Section<ccPS> p = new Section<ccPS>(MB, ccPS.Character_Height, 0, 32);

         string[] ps = new string[23];
         ps[0] = p.Get(ccPS.Character_Height, 3);                     // m.CharacterHeight = data[0];
         ps[1] = p.Get(ccPS.Ink_Drop_Use, 2);                         // m.InkDropUse = data[1];
         ps[2] = p.Get(ccPS.High_Speed_Print, 1);                     // m.HighSpeedPrinting = data[2];
         ps[3] = p.Get(ccPS.Character_Width, 3);                      // m.CharacterWidth = data[3];
         ps[4] = p.Get(ccPS.Character_Orientation, 1);                // m.CharacterOrientation = data[4];
         ps[5] = p.Get(ccPS.Ink_Drop_Charge_Rule, 1);                 // m.PrintingMethod = data[5];
         ps[6] = p.Get(ccPS.Print_Start_Delay_Forward, 4);            // m.ForwardDelay = data[6];
         ps[7] = p.Get(ccPS.Print_Start_Delay_Reverse, 4);            // m.ReverseDelay = data[7];
         ps[8] = p.Get(ccPS.Product_Speed_Matching, 1);               // m.ExternalEncoder = data[8] == "1";
         ps[9] = p.Get(ccPS.Line_Speed, 1);                           // m.LineSpeed = data[9];
         ps[10] = p.Get(ccPS.Pulse_Rate_Division_Factor, 4);          // m.Divisor = data[10];
         ps[11] = p.Get(ccPS.Print_Target_Width, 1);                  // m.PrintTargetWidth = data[11];
         ps[12] = p.Get(ccPS.Actual_Print_Width, 1);                  // m.PrintWidth = data[12];
         ps[13] = p.Get(ccPS.Speed_Compensation, 1);                  // m.SpeedCompensation = data[13];
         ps[14] = p.Get(ccPS.Speed_Compensation_Fine_Control, 1);     // m.SpeedCompensationFineControl = data[14];
         ps[15] = p.Get(ccPS.HeadToWorkDistance, 2);                  // m.HeadToWorkDistance = data[15];
         ps[16] = "0";                                                // m.NonDisclosureInfo = data[16];
         ps[17] = p.Get(ccPS.Repeat_Count, 4);                        // m.RepeatCount = data[17];
         ps[18] = p.Get(ccPS.Repeat_Interval, 4);                     // m.RepeatInterval = data[18];
         ps[19] = p.Get(ccPS.Target_Sensor_Timer, 3);                 // m.TargetSensorTimer = data[19];
         ps[20] = p.Get(ccPS.Target_Sensor_Filter, 1);                // m.TargetSensorFilter = data[20];
         ps[21] = p.Get(ccPS.Target_Sensor_Filter_Value, 4);          // m.TargetSensorSetupValue = data[21];
         ps[22] = p.Get(ccPS.Ink_Drop_Charge_Rule, 1);                // m.InkDropChargeRule = data[22];

         for (int i = 0; i < ps.Length; i++) {
            sb.Append(sESC2 + ps[i]);
         }

         return sb.Append(sETX).ToString();
      }

      private string VariousPrintSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         Section<ccSR> pf = new Section<ccSR>(MB, ccSR.Start_Year, 0, 0);

         return sb.Append(sETX).ToString();
      }

      // Comolete
      private string MessageGroupNames() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);
         int grps = 100;                                                            // Get maximum number of groups allowed
         int regCnt = (grps + 15) / 16;                                             // Last one is a partial
         Section<ccMG> mm = new Section<ccMG>(MB, ccMG.Registration, 0, regCnt);    // 16 registrations per word
         int[] regs = mm.GetWords(0);
         for (int i = 0; i < regs.Length; i++) {
            if (regs[i] != 0) {                                                     // Any on this block?
               for (int j = 0; j < 16; j++) {
                  int n = 15 - j;
                  if ((regs[i] & (1 << n)) > 0) {
                     n = i * 16 + j;                                                // 0-origin
                     MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
                     MB.SetAttribute(ccIDX.Group_Number, n);                        // Load the message into input registers
                     MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
                     Section<ccMG> msg = new Section<ccMG>(MB, ccMG.Group_Number, 0, 13);
                     sb.Append(sESC2);
                     sb.AppendFormat(msg.Get(ccMG.Group_Number, 2));
                     sb.AppendFormat(msg.Get(ccMG.Group_Name));
                  }
               }
            }
         }
         return sb.Append(sETX).ToString();
      }

      // Complete
      private string PrintData() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);
         int msgs = MB.GetDecAttribute(ccUI.Maximum_Registered_Message_Count);      // Get maximum number of messages allowed
         Section<ccMM> mm = new Section<ccMM>(MB, ccMM.Registration, 0, msgs / 16); // 16 registrations per word
         int[] regs = mm.GetWords(0);
         for (int i = 0; i < regs.Length; i++) {
            if (regs[i] != 0) {                                                     // Any on this block?
               for (int j = 0; j < 16; j++) {
                  int n = 15 - j;
                  if ((regs[i] & (1 << n)) > 0) {
                     n = i * 16 + j;                                                // 1-origin
                     MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);
                     MB.SetAttribute(ccIDX.Message_Number, n + 1);                  // Load the message into input registers
                     MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
                     Section<ccMM> msg = new Section<ccMM>(MB, ccMM.Message_Number, 0, 14);
                     sb.Append(sESC2);
                     sb.AppendFormat(msg.Get(ccMM.Message_Number, 4));
                     sb.AppendFormat(msg.Get(ccMM.Group_Number, 2));
                     sb.Append(msg.Get(ccMM.Message_Name));
                  }
               }
            }
         }

         return sb.Append(sETX).ToString();
      }

      // Complete (as needed but need to be extended by printer type.)
      private string UserEnvironmentSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);
         string empty = sESC2 + "0";
         sb.Append(empty); // Repeat print sensor mode
         sb.Append(empty); // Change mode
         sb.Append(empty); // Reverse print
         sb.Append(empty); // Speed compensation
         sb.Append(empty); // Print signal type
         sb.Append(empty); // Print data changeover error
         sb.Append(empty); // Create message
         sb.Append(sESC2 + "1"); // Character size menu 1
         sb.Append(sESC2 + "2"); // Character size menu 2
         sb.Append(empty); // Excitation V_Ref warning
         sb.Append(empty); // Print character one by one

         return sb.Append(sETX).ToString();
      }

      // Complete
      private string DateTimeSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         Section<ccES> dt = new Section<ccES>(MB, ccES.Current_Time_Year, 0, 18);

         sb.Append(sESC2);                               // Header
         sb.Append(dt.Get(ccES.Current_Time_Year, 4));   // Current time
         sb.Append(dt.Get(ccES.Current_Time_Month, 2));
         sb.Append(dt.Get(ccES.Current_Time_Day, 2));
         sb.Append(dt.Get(ccES.Current_Time_Hour, 2));
         sb.Append(dt.Get(ccES.Current_Time_Minute, 2));
         sb.Append(dt.Get(ccES.Current_Time_Second, 2));

         sb.Append(sESC2);                               // Header
         sb.Append(dt.Get(ccES.Calendar_Time_Control, 1));

         sb.Append(sESC2);                               // Header
         sb.Append(dt.Get(ccES.Calendar_Time_Year, 4));   // Calendar time
         sb.Append(dt.Get(ccES.Calendar_Time_Month, 2));
         sb.Append(dt.Get(ccES.Calendar_Time_Day, 2));
         sb.Append(dt.Get(ccES.Calendar_Time_Hour, 2));
         sb.Append(dt.Get(ccES.Calendar_Time_Minute, 2));
         sb.Append(dt.Get(ccES.Calendar_Time_Second, 2));

         sb.Append(sESC2);                               // Header
         sb.Append(dt.Get(ccES.Clock_System, 1));

         return sb.Append(sETX).ToString();
      }

      private string CommunicationsSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string TouchScreenSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string UnitInformation() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string OperationManagement() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      // Complete
      private string AlarmHistory() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         int errCount = MB.GetDecAttribute(ccAH.Message_Count);                 // Number of errors
         Section<ccAH> ah = new Section<ccAH>(MB, ccAH.Year, 0, errCount * 8);  // 
         for (int i = 0; i < errCount; i++) {
            ah.SetOffset(i);                                // Set to Alarm Code entry
            sb.Append(sESC2);                               // Header
            sb.Append(ah.Get(ccAH.Year, 4));                // Year
            sb.Append(ah.Get(ccAH.Month, 2));               // Month
            sb.Append(ah.Get(ccAH.Day, 2));                 // Day
            sb.Append(ah.Get(ccAH.Hour, 2));                // Hour
            sb.Append(ah.Get(ccAH.Minute, 2));              // Minute
            sb.Append(ah.Get(ccAH.Fault_Number, 3));        // Alarm Code
         }

         return sb.Append(sETX).ToString();
      }

      private string PartsUsageTime() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string CirculationSystemSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      // Complete
      private string SoftwareVersion() {
         StringBuilder sb = new StringBuilder(2000);
         sb.Append(sSTX);
         AttrData block = MB.GetAttrData(ccUI.Basic_Software_Version).Clone();
         int n = (0x0E40 - block.Val) / 32;

         string[] s = new string[n];
         byte[] ui;
         for (int i = 0; i < n; i++) {
            ui = MB.GetAttribute(block);
            s[i] = MB.FormatText(ui);
            block.Val += 32;
         }
         for (int i = 0; i < s.Length; i++) {
            if (!string.IsNullOrEmpty(s[i])) {
               sb.Append(sESC2);
               sb.Append((i + 1).ToString("D2"));
               sb.Append("00");
               sb.Append(s[i].Substring(1, 5));
            }
         }
         sb.Append(sETX);
         return sb.ToString();
      }

      private string AdjustmentOperationalCheckout() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string SolenoidValvePumpTest() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      // Complete
      private string FreeLayoutCoordinates() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         int[] cpi = GetTextLengths(out int itemCount, out int totalChars);
         for (int i = 0; i < itemCount; i++) {
            Section<ccPF> pf = new Section<ccPF>(MB, ccPF.Line_Count, i, 24);
            sb.Append(sDLE + (char)('1' + i));                                  // Item Header
            sb.Append(sESC2);
            sb.Append(pf.Get(ccPF.X_Coordinate, 5));
            sb.Append(sESC2);
            sb.Append(pf.Get(ccPF.Y_Coordinate, 2));
         }
         return sb.Append(sETX).ToString();
      }

      private string StirrerTest() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string MonthSubstituteRule() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string ViscometerCalibration() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      private string SystemEnvironmentSetup() {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         //Section<ccES> pf = new Section<ccES>(MB, ccES., 0, 0);

         return sb.Append(sETX).ToString();
      }

      private string UserPatternFixed(ModbusPkt pkt) {
         int[] logoLen = new int[] { // in bytes
            0,  // Unused
            8,  // Size4x5
            8,  // Size5x5
            8,  // Size5x7
            16,  // Size9x7
            16,  // Size7x10
            32,  // Size10x12
            32,  // Size12x16
            72,  // Size18x24
            128, // Size24x32
            32,  // Size11x11
            5,   // Size5x3_Chimney
            5,   // Size5x5_Chimney
            7,   // Size7x5_Chimney
            200, // Size30x40
            288, // Size36x48
         };
         int[] loc = new int[] { -1, 0,  37, 74, 111, 148, 185 };
         int[] cnt = new int[] { -1, 37, 37, 37,  37,  37,  15 };

         StringBuilder sb = new StringBuilder(sSTX, 2000);
         string DotMatrix = pkt.Data;
         int dotMatrix = Data.ToDropdownValue(MB.GetAttrData(ccIDX.User_Pattern_Size).Data, pkt.Data);
         if (dotMatrix >= 0) {
            MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 1);               // Liad the desired font
            MB.SetAttribute(ccIDX.User_Pattern_Size, DotMatrix);
            MB.SetAttribute(ccIDX.Start_Stop_Management_Flag, 2);
            int page = pkt.Page;
            Section<ccUP> up = new Section<ccUP>(MB, ccUP.User_Pattern_Fixed_Data, loc[page] * logoLen[dotMatrix] / 2, cnt[page] * logoLen[dotMatrix] / 2);
            string[] logos = up.GetUserPatterns(cnt[page]);
            for (int i = 0; i < cnt[page]; i++) {
               sb.Append(sESC2);
               sb.Append((loc[page] + i).ToString("D3"));
               sb.Append(logos[i]);
            }
         }

         return sb.Append(sETX).ToString();
      }

      private string UserPatternFree(ModbusPkt pkt) {
         StringBuilder sb = new StringBuilder(sSTX, 2000);

         return sb.Append(sETX).ToString();
      }

      // Data is not available 
      private string StandardCharacterPattern(ModbusPkt pkt) {
         throw new NotImplementedException();
      }



      #region ServiceRoutines

      private byte[][] GetMessageTextAsBytes() {                                // Returns attributed data
         int[] cpi = GetTextLengths(out int itemCount, out int totalChars);
         Section<ccPC> pc = new Section<ccPC>(MB, ccPC.Print_Character_String, 0, totalChars * 2); // Length in 2-word characters
         Byte[][] text = new byte[itemCount][];                                 // Break the text up by item
         int n = 0;
         for (int i = 0; i < cpi.Length; i++) {
            text[i] = pc.GetAttributedChrs(n, cpi[i]);
            n += cpi[i];
         }
         return text;
      }

      private int[] GetTextLengths(out int itemCount, out int totalChars) {
         itemCount = MB.GetDecAttribute(ccIDX.Number_Of_Items);  // Number of items in message (1 to 100)
         Section<ccPC> pb = new Section<ccPC>(MB, ccPC.Characters_per_Item, 0, itemCount);
         int[] cpi =  pb.GetWords(0, itemCount);
         totalChars = cpi.Sum();
         return cpi;
      }

      #endregion

   }

   #endregion

   #region Section Class

   public class Section<T> where T : Enum {

      byte[] b;
      int start;
      Modbus MB;
      AttrData baseAttr;
      int offset = 0;

      public Section(Modbus MB, T attr, int index, int Len) {
         this.MB = MB;                                      // Save Modbus I/O object
         baseAttr = MB.GetAttrData(attr);                   // Need the stride for indexing
         start = Convert.ToInt32(attr);                     // Get the base address of the block
         const int blockSize = 100;                         // Number of 2-byte items to read
         int n = (Len + blockSize) / blockSize;             // calculate number of reads needed
         if (n == 1) {                                      // If only one read needed, read it directly
            b = MB.GetAttributeBlock(attr, index, Len);
         } else {                                           // Otherwise, read it in block size chunks
            b = new byte[Len * 2];                          // Big enough for all reads
            int wordsRead = 0;
            for (int i = 0; i < n; i++) {                   // Read all sections and save then in array b
               int wordsToRead = Math.Min(blockSize, Len - wordsRead);
               byte[] t = MB.GetAttributeBlock(attr, index, wordsToRead, wordsRead);
               Buffer.BlockCopy(t, 0, b, wordsRead * 2, t.Length);
               wordsRead += wordsToRead;
            }
         }
      }

      public string Get(T item, int resultLen = -1) {
         string result = string.Empty;
         AttrData getAttr = MB.GetAttrData(item);           // Get attributes of desired item
         int n = (getAttr.Val - start) * 2 + offset;        // Get offset in byte array (2 bytes per word)
         int len = getAttr.Data.Len;                        // Get length
         switch (getAttr.Data.Fmt) {                        // Format the data as needed
            case DataFormats.None:
               break;
            case DataFormats.SDecimal:
            case DataFormats.Decimal:
               len += len;                                  // Length is in words so double it
               int res = 0;
               for (int i = 0; i < len; i++) {
                  res = (res << 8) + b[n + i];
               }

               result = res.ToString($"D{resultLen}");      // 
               break;
            case DataFormats.UTF8:
               char[] c = new char[len];
               Buffer.BlockCopy(b, n + 1, c, 0, len * 2 - 1); // Characters are stored as little endian.
               result = new string(c);
               break;
            case DataFormats.Date:
               break;
            case DataFormats.Bytes:
               break;
            case DataFormats.AttrText:
               break;
            default:
               break;
         }
         return result;
      }

      public int GetDec(T item) {
         AttrData getAttr = MB.GetAttrData(item);           // Get attributes of desired item
         int n = (getAttr.Val - start) * 2 + offset;        // Get offset in byte array (2 bytes per word)
         int len = getAttr.Data.Len;                        // Get length
         int res = 0;
         switch (getAttr.Data.Fmt) {                        // Format the data as needed
            case DataFormats.Decimal:
               len += len;                                  // Length is in words so double it
               for (int i = 0; i < len; i++) {
                  res = (res << 8) + b[n + i];
               }
               break;
            default:
               // Need msg here
               break;
         }
         return res;
      }

      public byte[] GetBytes(int offset, int len) {
         byte[] response = new byte[len];
         Buffer.BlockCopy(b, offset, response, 0, len);
         return response;
      }

      public int[] GetWords(int offset, int len = -1) {          // offset and len in words
         if (len == -1) {
            len = b.Length / 2 - offset;
         }
         int[] response = new int[len];
         for (int i = 0; i < len; i++) {
            int n = (offset + i) * 2;
            response[i] = (b[n] << 8) + b[n + 1];       // Values are stored Big Endian
         }
         return response;
      }

      public byte[] GetAttributedChrs(int offset, int len) {          // Offset and Len in Attributed Characters (4-byte characters)
         byte[] response = new byte[len * 4];
         Buffer.BlockCopy(b, offset * 4, response, 0, len * 4);
         return response;
      }

      public string[] GetUserPatterns(int count) {
         string[] result = new string[count];               // Patterns are returned as strings
         int n = b.Length / count;                          // Number of bytes per string
         char[] c = new char[n];                            // A place to stage the output
         int k = 0;
         for (int i = 0; i < count; i++) {                  // Repeat for each logo
            for (int j = 0; j < n; j++) {
               c[j] = (char)b[k++];
            }
            result[i] = new string(c);
         }
         return result;
      }

      public void SetOffset(int n) {
         offset = baseAttr.Stride * n * 2;                  // For arrays within the block
      }

   }

   #endregion
}
