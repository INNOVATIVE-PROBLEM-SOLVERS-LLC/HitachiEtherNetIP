# HitachiEtherNetIP

Update -- Jun 10, 2020 -- This implementation is being developed using an Hitachi UX-161 (Runs on a Linux based platform).  When testing began for Modbus on the UX-160, RX-2, and RX platforms, it was discovered that there is a difference in the Modbus implementation between the Linux based platforms and previous platforms.  For the legacy platforms, each command sent from the PC to the printer receives an immediate ACK response from the printer.  After the printer sends the response to the PC, it waits for an ACK.  On the Linux platform, the ACK is not required.

The Linux based platform has a RJ-45 connector for direct communication to the printer.  The legacy platforms interface thru a TUP-I device.  This may account for the differences in the implementations.  I need to connect a TUP-I device to my UX-161 and see if I get the same result.

My implementation uses "TcpClient" and "NetworkStream" for Synchronous I/O to the printer.  This implementation does not send a ACK for the response frame sent from the printer to the PC.  The printer waits for the ACK and the next I/O operation times out.  I have yet to find a way to send the ACK without going to a full "TcpClient" implementation.  Anyone with an idea is welcome to speak up.

Update -- Apr 12, 2020 -- Level 3.03 of the UX-161 printer software is available.  However, no issues with Modbus or EtherNet/IP were addressed.  Level 2.07 is the best level for these applications.

Update -- Mar 25, 2020 -- All I/O was happening on the main thread leaving long pauses for viewing other tabs.  The I/O was moved to a background thread and is queue driven.  Also, the twin nozzle application was removed since it was no longer being used.

Update -- Mar 18, 2020 -- Time Count and Shifts were not structured properly.  Previously, Time Count and Shifts we processed as printer wide.  They have been moved in as part of the calendar block. 

Update -- Mar 17, 2020 -- Avoid the version with the tag "Level_8.0.0.2".  An attempt to consolidate code resulted in logging being broken.

Update -- MAR 14, 2020 -- The version with the tag "Level_8.0.0.0" has significate updates to "Modbus161" and "ModbusDLL".  With this version, a XML message containing five calendar blocks, a counter block, and a user pattern was successfully sent to the printer, retrieved from the printer, and compared as an exact match.  An inspection of the printer revealed that the message was properly built.

UPDATE -- Hitachi has released UX-161 code that is based on Revision 7 of Hitachi's EtherNet/IP implementation.  The document "<b>EtherNetIP_UsersManual_7th.pdf</b>" is included.  The implementation supports two communication protocols: Message and Cyclic. The document describes the Cyclic communications.  It is assumed that the Message Communications of Revision 4 documentation still applies but that is not clear.  It is also not clear how to switch from Cyclic to Message modes.   All Get requests have to be in their own Forward Envelope.  Have not successfully implemented any Set or Service operation.

If you use level 3.01 with the Browser, be sure to set the language from the Dropdown.

EthetNet/IP interface to Hitachi EtherNet/IP enabled printers. And, Hitachi Protocol (Serial) for Hitachi Model PH thru Model UX-161 printers.

The EtherNet/IP implementation is based on Revision 4 of Hitachi's EtherNet/IP implementation.  The document "<b>EtherNetIP_UsersManual_4th.pdf</b>" is included.

A brief user's guide for the EIP DLL <b>"Hitachi EtherNetIP Developer Guide.pdf</b>" is also included.  The guide now includes a implementation of an Hitachi Printer message defined in XML format.

The Hitachi Protocol (Serial) implementation is based on <b>"RX_Technical_Manual_revD.pdf"</b> is included.

The goal is to develop an application that is as flexible as possible in accessing all the features of the printer.  At the same time,  hiding as much as possible of the mapping between the machine codes and human readable values.  For example, Fonts and Bar Codes.

The solution is broken into eight projects:
<ul>
  <li>Hitachi_EIP_DLL -- A Class Library "EIP" that is the interface to the Hitachi printer.</li>
  <li>Hitachi_Browser_DLL -- A Class Library "Browser" that uses the "EIP" DLL to browse the Hitachi printer.</li>
  <li>Hitachi_Browser_Test -- A test program the instantiates the "Browser" DLL to open up a browser.</li>
  <li>Hitachi_EIP_Test -- A starting point for developing an application that uses the "Browser" and "EIP" DLLs.</li>
  <li>HitachiProtocol -- A Class Library "HitachiPrinter" for the Hitachi Protocol interface to all printers.</li>
  <li>HitachiProtocol_Test -- A test program the instantiates the "HitachiPrinter" Class and shows simple commands.</li>
  <li>IJPLib_Test -- A test program to drive the IJPLibXML DLL.</li>
  <li>IJPLibXML -- A Class Library that uses EIPLib/Modbus to send/receive messages in XML format.</li>
  <li>Modbus_DLL -- A Class Library that uses Modbus to send/receive messages in XML format.</li>
  <li>Modbus161 -- A test program to drive the Modbus_DLL.</li>
</ul>

A record of all traffic to/from the printer is kept in an Excel Spreadsheet.  You will need Microsoft Office installed to execute the application as it is.

It is still a work in progress so enjoy.
