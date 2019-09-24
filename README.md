# HitachiEtherNetIP
EthetNet/IP interface to Hitachi EtherNet/IP enabled printers. And, Hitachi Protocol (Serial) for Hitachi Model PH thru Model UX-161 printers.

Warning:  This is my first attempt at EtherNet/IP programming.  Please verfy, then try.  <b>That said, anyone who would like to join the development should contact me.</b>

The EtherNet/IP implementation is based on Revision 4 of Hitachi's EtherNet/IP implementation.  The document "<b>EtherNetIP_UsersManual_4th.pdf</b>" is included.

A brief user's guide for the EIP DLL <b>"Hitachi EtherNetIP Developer Guide.pdf</b>" is also included.

The Hitachi Protocol (Serial) implementation is based on <b>"RX_Technical_Manual_revD.pdf"</b> is included.

The goal is to develop an application that is as flexible as possible in accessing all the features of the printer.  At the same time,  hiding as much as possible of the mapping between the machine codes and human readable values.  For example, Fonts and Bar Codes.

The solution is broken into six projects:
<ul>
  <li>Hitachi_EIP_DLL -- A Class Library "EIP" that is the interface to the Hitachi printer.</li>
  <li>Hitachi_Browser_DLL -- A Class Library "Browser" that uses the "EIP" DLL to browse the Hitachi printer.</li>
  <li>Hitachi_Browser_Test -- A test program the instantiates the "Browser" DLL to open up a browser.</li>
  <li>Hitachi_EIP_Test -- A starting point for developing an application that uses the "Browser" and "EIP" DLLs.</li>
  <li>HitachiProtocol -- A Class Library "HitachiPrinter" for the Hitachi Protocol interface to all printers.</li>
  <li>HitachiProtocol_Test -- A test program the instantiates the "HitachiPrinter" Class and shows simple commands.</li>
</ul>

A record of all traffic to/from the printer is kept in an Excel Spreadsheet.  You will need Microsoft Office installed to execute the application as it is.

It is still a work in progress so enjoy.
