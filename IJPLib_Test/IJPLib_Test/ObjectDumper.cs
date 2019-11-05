using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace IJPLib_Test {
   public class ObjectDumper {
      TreeNode root = null;
      TreeNode rootBase = null;
      TreeNode t;
      private int _level;
      private readonly int _indentSize;
      private readonly StringBuilder _stringBuilder;
      private readonly List<int> _hashListOfFoundElements;

      bool skipNode = false;

      public ObjectDumper(int indentSize) {
         _indentSize = indentSize;
         _stringBuilder = new StringBuilder();
         _hashListOfFoundElements = new List<int>();
      }

      public void Dump(object element, out string indentedText, out TreeNode treeView ) {
         string result = DumpElement(element);
         indentedText = result;
         treeView = rootBase;
      }

      public string DumpElement(object element) {
         if (element == null || element is ValueType || element is string) {
            Write(FormatValue(element));
         } else {
            var objectType = element.GetType();
            if (!typeof(IEnumerable).IsAssignableFrom(objectType)) {
               skipNode = true;
               Write("{{{0}}}", objectType.FullName);
               _hashListOfFoundElements.Add(element.GetHashCode());
               _level++;
               t = new TreeNode(string.Format("{{{0}}}", objectType.FullName));
               if (root == null) {
                  rootBase = t;
               } else {
                  root.Nodes.Add(t);
               }
               root = t;
            }

            var enumerableElement = element as IEnumerable;
            if (enumerableElement != null) {
               foreach (object item in enumerableElement) {
                  if (item is IEnumerable && !(item is string)) {
                     _level++;
                     t = new TreeNode();
                     root.Nodes.Add(t);
                     root = t;
                     skipNode = true;
                     DumpElement(item);
                     _level--;
                     root = root.Parent;
                  } else {
                     if (!AlreadyTouched(item))
                        DumpElement(item);
                     else
                        Write("{{{0}}} <-- bidirectional reference found", item.GetType().FullName);
                  }
               }
            } else {
               MemberInfo[] members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
               foreach (var memberInfo in members) {
                  var fieldInfo = memberInfo as FieldInfo;
                  var propertyInfo = memberInfo as PropertyInfo;

                  if (fieldInfo == null && propertyInfo == null)
                     continue;

                  var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                  object value = fieldInfo != null
                                     ? fieldInfo.GetValue(element)
                                     : propertyInfo.GetValue(element, null);

                  if (type.IsValueType || type == typeof(string)) {
                     Write("{0}: {1}", memberInfo.Name, FormatValue(value));
                  } else {
                     var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                     skipNode = true;
                     Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                     var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                     _level++;
                     t = new TreeNode(string.Format("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }"));
                     root.Nodes.Add(t);
                     root = t;
                     if (!alreadyTouched)
                        DumpElement(value);
                     else
                        Write("{{{0}}} <-- bidirectional reference found", value.GetType().FullName);
                     _level--;
                     root = root.Parent;
                  }
               }
            }

            if (!typeof(IEnumerable).IsAssignableFrom(objectType)) {
               _level--;
               root = root.Parent;
            }
         }

         return _stringBuilder.ToString();
      }

      private bool AlreadyTouched(object value) {
         if (value == null)
            return false;

         var hash = value.GetHashCode();
         for (var i = 0; i < _hashListOfFoundElements.Count; i++) {
            if (_hashListOfFoundElements[i] == hash)
               return true;
         }
         return false;
      }

      private void Write(string value, params object[] args) {
         var space = new string(' ', _level * _indentSize);

         if (args != null)
            value = string.Format(value, args);

         _stringBuilder.AppendLine(space + value);
         if (!skipNode) {
            if (root == null) {
               root = new TreeNode(value);
               rootBase = root;
            }
            root.Nodes.Add(value);
         }
         skipNode = false;
      }

      private string FormatValue(object o) {
         if (o == null)
            return ("null");

         if (o is DateTime)
            return (((DateTime)o).ToShortDateString());

         if (o is string)
            return string.Format("\"{0}\"", o);

         if (o is char && (char)o == '\0')
            return string.Empty;

         if (o is ValueType)
            return (o.ToString());

         if (o is IEnumerable)
            return ("...");

         return ("{ }");
      }
   }
}