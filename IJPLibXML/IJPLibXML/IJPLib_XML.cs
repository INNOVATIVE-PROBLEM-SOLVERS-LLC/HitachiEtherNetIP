using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HIES.IJP.RX;

namespace IJPLibXML {


   public class IJPLib_XML {

      #region Events and properties

      // Event Logging
      public event LogHandler Log;
      public delegate void LogHandler(IJPLib_XML sender, string msg);

      // Task Completion
      public event CompleteHandler Complete;
      public delegate void CompleteHandler(IJPLib_XML sender, IX_EventArgs evArgs);

      public bool IsConnected { get { return ijp != null; } }
      public IJPOnlineStatus ComStatus { get { return ijp != null ? ComOn : IJPOnlineStatus.Offline; } }


      #endregion

      #region Data Declarations

      public const int FirstFixedUP = 0xF140;
      public const int LastFixedUP = 0xF208;
      public const int FirstFreeUP = 0xF209;
      public const int LastFreeUP = 0xF23A;

      public enum ReqType {
         Connect,
         Disconnect,
         ClearMessage,
         NewMessage,
         GetMessage,
         GetXML,
         GetXMLOnly,
         GetObjectSettings,
         GetDirectory,
         GetSettings,
         SetXML,
         SetMessage,
         SetComStatus,
         CallMessage,
         SaveMessage,
         Exit,
      }

      // Do the work in the background
      Thread t;

      // Use Blocking Collection to avoid spin waits
      public BlockingCollection<ReqPkt> Tasks = new BlockingCollection<ReqPkt>();
      ReqPkt pkt;

      public IJP ijp;
      IJPOnlineStatus ComOn = IJPOnlineStatus.Offline;

      IJPMessage message = null;

      Form parent;

      #endregion

      #region Constructors and destructors

      public IJPLib_XML(Form parent) {
         this.parent = parent;
         t = new Thread(processTasks);
         t.Start();
      }

      #endregion

      #region Task Processing

      // Main processing loop
      private void processTasks() {
         while (true) {
            pkt = Tasks.Take();
            if (pkt.Type == ReqType.Exit) {
               break;
            }
            parent.BeginInvoke(new EventHandler(delegate { Log(this, pkt.Type.ToString() + " Starting!"); }));
            IX_EventArgs evArgs = new IX_EventArgs(pkt.Type);
            MsgToXml mtx = null;
            try {
               switch (pkt.Type) {
                  case ReqType.Connect:
                     Connect(pkt);
                     evArgs.ijp = ijp;
                     evArgs.ComStatus = ComOn;
                     break;
                  case ReqType.Disconnect:
                     Disconnect(pkt);
                     evArgs.ijp = ijp;
                     evArgs.ComStatus = ComOn;
                     break;
                  case ReqType.ClearMessage:
                     message = null;
                     evArgs.message = message;
                     break;
                  case ReqType.NewMessage:
                     message = new IJPMessage();
                     evArgs.message = message;
                     break;
                  case ReqType.GetMessage:
                     message = (IJPMessage)ijp.GetMessage();
                     evArgs.message = message;
                     break;
                  case ReqType.GetXML:
                     if (message != null) {
                        mtx = new MsgToXml();
                        string xml = mtx.RetrieveXML(message, ijp, pkt.MessageInfo);
                        ProcessLabel(xml, out string IndentedXML, out TreeNode tnXML);
                        evArgs.Indented = IndentedXML;
                        evArgs.TreeNode = tnXML;
                     }
                     break;
                  case ReqType.GetXMLOnly:
                     if (message != null) {
                        mtx = new MsgToXml();
                        evArgs.Indented = mtx.RetrieveXML(message, ijp, pkt.MessageInfo);
                     }
                     break;
                  case ReqType.GetObjectSettings:
                     if (message != null) {
                        ObjectDumper od = new ObjectDumper(2);
                        od.Dump(message, out string indentedView, out TreeNode treeNode);
                        evArgs.Indented = indentedView;
                        evArgs.TreeNode = treeNode;
                     }
                     break;
                  case ReqType.GetDirectory:
                     evArgs.mi = ijp.ListMessage(pkt.Start, pkt.End);
                     break;
                  case ReqType.GetSettings:
                     message = (IJPMessage)ijp.GetMessage();
                     break;
                  case ReqType.SetXML:
                     XmlToMsg xtm = new XmlToMsg(pkt.XML, ijp);
                     //xtm.Log += Log;
                     message = xtm.BuildMessage();
                     //xtm.Log -= Log;
                     //Log($"Run XML Test Complete");
                     evArgs.message = message;
                     break;
                  case ReqType.SetComStatus:
                     ijp.SetComPort(pkt.ComStatus);
                     evArgs.ComStatus = pkt.ComStatus;
                     break;
                  case ReqType.SetMessage:
                     if (message != null) {
                        ijp.SetMessage(message);
                     }
                     break;
                  case ReqType.CallMessage:
                     ijp.CallMessage(pkt.MessageNumber);
                     break;
                  case ReqType.SaveMessage:
                     ijp.SaveMessage(pkt.MessageInfo);
                     break;
               }
            } catch (Exception e) {
               parent.BeginInvoke(new EventHandler(delegate { Log(this, $"IJP_XML: {e.Message} \n{e.StackTrace}"); }));
            }
            parent.BeginInvoke(new EventHandler(delegate { Complete(this, evArgs); }));
         }
      }

      // Connect to printer
      private void Connect(ReqPkt pkt) {
         Disconnect(pkt);
         ijp = new IJP();
         ijp.IPAddress = pkt.ipAddress;
         ijp.Timeout = pkt.timeOut;
         ijp.Retry = pkt.retries;
         ijp.Connect();
         ComOn = ijp.GetComPort();
         // Be sure com is on
         if (ComOn == IJPOnlineStatus.Offline) {
            ijp.SetComPort(IJPOnlineStatus.Online);
            ComOn = IJPOnlineStatus.Online;
         }
      }

      // Disconnect from printer
      private void Disconnect(ReqPkt pkt) {
         if (null != this.ijp) {
            ijp.Disconnect();
            ijp.Dispose();
            ijp = null;
            ComOn = IJPOnlineStatus.Offline;
         }
      }

      #endregion

      #region XML Label Processing

      // Process an XML Label
      public bool ProcessLabel(string xml, out string indentedXML, out TreeNode tnXML) {
         indentedXML = string.Empty;
         tnXML = null;
         XmlDocument xmlDoc;
         bool result = false;
         try {
            // Can be called with a Filename or XML text
            int xmlStart = xml.IndexOf("<Label");
            int xmlEnd = xml.IndexOf("</Label>", xmlStart + 7);
            if (xmlEnd > 0) {
               xml = xml.Substring(xmlStart, xmlEnd - xmlStart + 8);
               xmlDoc = new XmlDocument() { PreserveWhitespace = true };
               xmlDoc.LoadXml(xml);
               xmlStart = xml.IndexOf("<Label");
               if (xmlStart >= 0) {
                  indentedXML = xml.Substring(xmlStart);
                  tnXML = new TreeNode(xmlDoc.DocumentElement.Name);
                  AddNode(xmlDoc.DocumentElement, tnXML);
                  result = true;
               }
            }
         } catch (Exception e) {
            if (parent.InvokeRequired) {
               parent.BeginInvoke(new EventHandler(delegate { Log(this, $"IJP_XML: {e.Message} \n{e.StackTrace}"); }));
            }
         }
         return result;
      }

      // Add a node to the tree view
      private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode) {
         if (inXmlNode is XmlWhitespace)
            return;
         XmlNode xNode;
         XmlNodeList nodeList;
         if (inXmlNode.HasChildNodes) {
            inTreeNode.Text = GetNameAttr(inXmlNode);
            nodeList = inXmlNode.ChildNodes;
            int j = 0;
            for (int i = 0; i < nodeList.Count; i++) {
               xNode = inXmlNode.ChildNodes[i];
               if (xNode is XmlWhitespace)
                  continue;
               if (xNode.Name == "#text") {
                  inTreeNode.Text = inXmlNode.OuterXml.Trim();
               } else {
                  if (!(xNode is XmlWhitespace)) {
                     inTreeNode.Nodes.Add(new TreeNode(GetNameAttr(xNode)));
                     AddNode(xNode, inTreeNode.Nodes[j]);
                  }
               }
               j++;
            }
         } else {
            inTreeNode.Text = inXmlNode.OuterXml.Trim();
         }
      }

      // Get the attributes associated with a node
      private string GetNameAttr(XmlNode n) {
         string result = n.Name;
         if (n.Attributes != null && n.Attributes.Count > 0) {
            foreach (XmlAttribute attribute in n.Attributes) {
               result += $" {attribute.Name}=\"{attribute.Value}\"";
            }
         }
         return result;
      }

      #endregion

   }

   #region Request Packet and Event Args 

   public class ReqPkt : IFormattable {

      public IJPLib_XML.ReqType Type { get; set; }
      public string ipAddress { get; set; } = "10.0.0.100";
      public int timeOut { get; set; } = 5000;
      public int retries { get; set; } = 5;
      public IJPOnlineStatus ComStatus { get; set; }
      public ushort MessageNumber { get; set; } = 0;
      public int Start { get; set; }
      public int End { get; set; }
      public string XML { get; set; }
      public IJPMessageInfo MessageInfo {get; set;}

      public ReqPkt(IJPLib_XML.ReqType Type) {
         this.Type = Type;
      }

      public string ToString(string format, IFormatProvider provider) {
         string result = $"{Type}:";
         switch (this.Type) {
            case IJPLib_XML.ReqType.Connect:
               result = $"{Type}: IP Address {ipAddress}, Timeout {timeOut}, Retries {retries}";
               break;
            case IJPLib_XML.ReqType.Disconnect:
               break;
            case IJPLib_XML.ReqType.GetMessage:
               break;
            case IJPLib_XML.ReqType.GetDirectory:
               break;
            case IJPLib_XML.ReqType.GetXML:
               break;
            case IJPLib_XML.ReqType.GetSettings:
               break;
            case IJPLib_XML.ReqType.SetComStatus:
               break;
            case IJPLib_XML.ReqType.Exit:
               break;
            default:
               break;
         }
         return result;
      }
   }

   public class IX_EventArgs : EventArgs {

      public IJPLib_XML.ReqType Type { get; set; }
      public IJP ijp { get; set; } = null;
      public IJPOnlineStatus ComStatus { get; set; }
      public IJPMessage message { get; set; } = null;
      public string Indented { get; set; } = null;
      public TreeNode TreeNode { get; set; } = null;
      public IIJPMessageInfo[] mi { get; set; } = null;

      public IX_EventArgs(IJPLib_XML.ReqType Type) {
         this.Type = Type;
      }

   }

   #endregion

}
